using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextPool<GymDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // configure options.Password, options.Lockout, etc.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<GymDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHostedService<SubscriptionExpiryService>();
builder.Services.AddHostedService<AutoCheckoutService>();
builder.Services.AddScoped<IQRTokenService, QRTokenService>();

// JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key not found in configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// --- CORS ---
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            // Permissive in dev only — never in production
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// --- Caching ---
builder.Services.AddMemoryCache();
builder.Services.AddOutputCache(options =>
{
    // Classes list: rarely mutated, 60 s TTL
    options.AddPolicy("classes", policy => policy
        .Expire(TimeSpan.FromSeconds(60))
        .Tag("classes"));

    // Rooms list + total: rarely mutated, 60 s TTL
    options.AddPolicy("rooms", policy => policy
        .Expire(TimeSpan.FromSeconds(60))
        .Tag("rooms"));

    // Member list/total/recents: query string (page, pageSize) is part of
    // the default cache key, so paginated pages are cached independently
    options.AddPolicy("members", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("members"));

    // Staff list/total: same approach as members
    options.AddPolicy("staff", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("staff"));

    // Trainers: same lifecycle as staff
    options.AddPolicy("trainers", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("trainers"));

    // Branches and tiers almost never change — long TTL is safe
    options.AddPolicy("branches", policy => policy
        .Expire(TimeSpan.FromSeconds(120))
        .Tag("branches"));

    options.AddPolicy("tiers", policy => policy
        .Expire(TimeSpan.FromSeconds(120))
        .Tag("tiers"));

    // Bookings: moderate mutation rate
    options.AddPolicy("bookings", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("bookings"));

    // Payments: low mutation rate (members pay ~monthly), safe at 30 s
    options.AddPolicy("payments", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("payments"));

    // Attendance changes on every check-in/out — keep TTL very short
    options.AddPolicy("attendance", policy => policy
        .Expire(TimeSpan.FromSeconds(10))
        .Tag("attendance"));
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

await GymSystem.Api.Data.SeedData.EnsureRolesAndAdminAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Must come after auth so only authenticated responses are cached
app.UseOutputCache();

app.MapControllers();

app.Run();