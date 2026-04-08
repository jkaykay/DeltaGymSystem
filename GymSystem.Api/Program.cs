// ============================================================
// Program.cs — Application entry point and configuration.
// This file sets up all services (dependency injection),
// middleware (the request pipeline), and starts the web server.
// ============================================================

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

// Create the web application builder — this is where we configure
// all services before the app is built and started.
var builder = WebApplication.CreateBuilder(args);

// --- Database Configuration ---
// Read the database connection string from appsettings.json.
// If it's missing, the app throws an error immediately on startup.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register the Entity Framework database context with connection pooling.
// EnableRetryOnFailure makes the app automatically retry if the SQL Server
// connection drops temporarily (up to 5 times, waiting up to 30 seconds).
builder.Services.AddDbContextPool<GymDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// --- ASP.NET Identity Configuration ---
// Identity handles user accounts, passwords, and roles.
// Here we set password rules (must contain uppercase, lowercase, digit, symbol)
// and restrict which characters are allowed in usernames.
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
// Store user data in the database via Entity Framework
.AddEntityFrameworkStores<GymDbContext>()
// Add default token providers for password resets, email confirmation, etc.
.AddDefaultTokenProviders();

// --- Custom Service Registration (Dependency Injection) ---
// "Scoped" means a new instance is created per HTTP request.
// "HostedService" means a background task that runs continuously.
builder.Services.AddScoped<ITokenService, TokenService>();           // Generates JWT tokens for login
builder.Services.AddHostedService<SubscriptionExpiryService>();      // Background job: expires old subscriptions
builder.Services.AddHostedService<AutoCheckoutService>();            // Background job: auto-checks out members who forgot
builder.Services.AddScoped<IQRTokenService, QRTokenService>();      // Generates and validates QR code tokens

// Singleton — shares the same IMemoryCache instance already registered below
builder.Services.AddSingleton<ITokenRevocationService, TokenRevocationService>();

// --- JWT (JSON Web Token) Authentication ---
// JWT is a stateless way to authenticate API requests.
// The client sends a token in the "Authorization" header; the server validates it.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key not found in configuration.");

// Tell ASP.NET Core to use JWT Bearer as the default authentication method.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // These rules define what makes a token valid:
    // - It must come from our issuer, be intended for our audience,
    //   not be expired, and be signed with our secret key.
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
    // After a token passes validation, check if it was revoked (e.g. user logged out).
    // This lets us invalidate tokens before they naturally expire.
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
// MemoryCache stores data in RAM (used by token revocation and QR codes).
// OutputCache caches entire HTTP responses so repeated requests are served
// faster without hitting the database again. Each policy below defines
// how long a particular type of response is cached.
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

    options.AddPolicy("subscriptions", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .Tag("subscriptions"));
});

// Register MVC controllers (our API endpoints live in Controller classes).
builder.Services.AddControllers();
// Enable the OpenAPI (Swagger) endpoint for API documentation.
builder.Services.AddOpenApi();
// Register a typed HttpClient for the OpenRouter LLM service.
// AddHttpClient automatically manages the underlying HTTP connections.
builder.Services.AddHttpClient<IOpenRouterService, OpenRouterService>();

// --- Rate Limiting ---
// Rate limiting prevents clients from making too many requests in a short time.
// Each "limiter" below uses a fixed window: e.g. "llm" allows 5 requests per minute.
// If the limit is exceeded, the server responds with HTTP 429 (Too Many Requests).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // LLM chatbot: 5 requests per minute (AI calls are expensive)
    options.AddFixedWindowLimiter("llm", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    // Authentication (login/register): 10 requests per minute
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    // Booking endpoints: 10 requests per minute
    options.AddFixedWindowLimiter("booking", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    // Payment endpoints: 5 requests per minute
    options.AddFixedWindowLimiter("payment", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    // QR code endpoints: 15 requests per minute
    options.AddFixedWindowLimiter("qr", limiter =>
    {
        limiter.PermitLimit = 15;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });
});

// Build the application — all services are now registered and locked in.
var app = builder.Build();

// Seed the database with default roles (Admin, Staff, Trainer, Member)
// and create the initial admin account if one doesn't already exist.
await GymSystem.Api.Data.SeedData.EnsureRolesAndAdminAsync(app.Services);

// In development mode, expose the OpenAPI (Swagger) documentation endpoint.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- Middleware Pipeline ---
// Middleware runs in order for every HTTP request.
// The order matters — e.g. authentication must come before authorization.
app.UseHttpsRedirection();   // Redirect HTTP requests to HTTPS
app.UseCors();               // Apply Cross-Origin Resource Sharing rules
app.UseRateLimiter();        // Enforce request rate limits
app.UseAuthentication();     // Identify who the user is (via JWT token)
app.UseAuthorization();      // Check if the user has permission for the endpoint
app.UseOutputCache();        // Serve cached responses when available
app.MapControllers();        // Map controller routes (e.g. api/auth, api/member)

// Start the web server and begin listening for requests.
app.Run();