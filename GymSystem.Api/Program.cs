using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextPool<GymDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
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

// Singleton — shares the same IMemoryCache instance already registered below
builder.Services.AddSingleton<ITokenRevocationService, TokenRevocationService>();

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
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = ctx =>
        {
            var revocationService = ctx.HttpContext.RequestServices
                .GetRequiredService<ITokenRevocationService>();

            var jti = ctx.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (jti is not null && revocationService.IsRevoked(jti))
                ctx.Fail("Token has been revoked.");

            return Task.CompletedTask;
        }
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
    options.AddPolicy("classes", policy => policy
        .Expire(TimeSpan.FromSeconds(60))
        .Tag("classes"));

    options.AddPolicy("rooms", policy => policy
        .Expire(TimeSpan.FromSeconds(60))
        .Tag("rooms"));

    options.AddPolicy("members", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("members"));

    options.AddPolicy("staff", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("staff"));

    options.AddPolicy("trainers", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("trainers"));

    options.AddPolicy("branches", policy => policy
        .Expire(TimeSpan.FromSeconds(120))
        .Tag("branches"));

    options.AddPolicy("tiers", policy => policy
        .Expire(TimeSpan.FromSeconds(120))
        .Tag("tiers"));

    options.AddPolicy("bookings", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("bookings"));

    options.AddPolicy("payments", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("payments"));

    // Attendance changes on every check-in/out — keep TTL very short
    options.AddPolicy("attendance", policy => policy
        .Expire(TimeSpan.FromSeconds(10))
        .Tag("attendance"));

    options.AddPolicy("equipment", policy => policy
        .Expire(TimeSpan.FromSeconds(60))
        .Tag("equipment"));

    options.AddPolicy("schedules", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("schedules"));
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<IOpenRouterService, OpenRouterService>();

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("llm", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("booking", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("payment", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("qr", limiter =>
    {
        limiter.PermitLimit = 15;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });
});

var app = builder.Build();

await GymSystem.Api.Data.SeedData.EnsureRolesAndAdminAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.MapControllers();

app.Run();