// Program.cs is the entry point of the web application.
// It configures all the services (dependency injection), middleware, and routing
// before starting the web server.

using GymSystem.Web.Services;

// Create a builder that lets us configure the app before it runs.
var builder = WebApplication.CreateBuilder(args);

// --- MVC with Area support ---
// Adds support for Controllers and Razor Views so we can serve HTML pages.
builder.Services.AddControllersWithViews();

// --- Required for TokenDelegatingHandler ---
// IHttpContextAccessor lets services access the current HTTP request (e.g. to read cookies).
builder.Services.AddHttpContextAccessor();
// TokenDelegatingHandler is a custom handler that automatically attaches the JWT token
// to every outgoing API request so the backend knows which user is making the call.
builder.Services.AddTransient<TokenDelegatingHandler>();

// --- API service registration (Dependency Injection) ---
// Each "AddScoped" line tells the DI container: "when a controller asks for the interface,
// give it the matching implementation". Scoped means one instance per HTTP request.
builder.Services.AddScoped<IManagementApiService, ManagementApiService>();
builder.Services.AddScoped<IMemberApiService, MemberApiService>();
builder.Services.AddScoped<IAuthApiService, AuthApiService>();
builder.Services.AddScoped<ITrainerApiService, TrainerApiService>();

// --- Typed HttpClient for API consumption ---
// Creates a named HttpClient ("GymApi") that all API services share.
// The base URL comes from appsettings.json so we only configure it once.
// TokenDelegatingHandler is chained in so every request gets the Bearer token automatically.
builder.Services.AddHttpClient("GymApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["GymApi:BaseUrl"]
        ?? throw new InvalidOperationException("GymApi:BaseUrl is not configured."));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<TokenDelegatingHandler>();

// --- Authentication (cookie-based, tokens stored in session/cookie) ---
// The app uses cookie authentication. When a user logs in, a cookie is created
// that stores their identity (claims) and JWT token. The cookie is sent with
// every request so the server knows who the user is.
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        // Default paths for login/logout/access-denied when no area is specified.
        options.LoginPath = "/Member/Login/Index";
        options.LogoutPath = "/Member/Logout";
        options.AccessDeniedPath = "/Member/Login/AccessDenied";

        // Custom redirect logic: when an unauthenticated user tries to access a protected page,
        // redirect them to the correct login page based on which area they were trying to reach.
        options.Events.OnRedirectToLogin = ctx =>
        {
            var returnUrl = ctx.Request.Path + ctx.Request.QueryString;

            if (ctx.Request.Path.StartsWithSegments("/Management", StringComparison.OrdinalIgnoreCase))
            {
                var redirect = $"/Management/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            else if (ctx.Request.Path.StartsWithSegments("/Member", StringComparison.OrdinalIgnoreCase))
            {
                var redirect = $"/Member/Login/Index?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            else if (ctx.Request.Path.StartsWithSegments("/Trainer", StringComparison.OrdinalIgnoreCase))
            {
                var redirect = $"/Trainer/Auth?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            else
            {
                var redirect = $"/Login/Index?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            return Task.CompletedTask;
        };

        // Same idea but for access-denied: if a logged-in user lacks permission,
        // redirect them to the appropriate "Access Denied" page for their area.
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            var returnUrl = ctx.Request.Path + ctx.Request.QueryString;
            if (ctx.Request.Path.StartsWithSegments("/Management", StringComparison.OrdinalIgnoreCase))
            {
                var redirect = $"/Management/Account/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            else if (ctx.Request.Path.StartsWithSegments("/Member", StringComparison.OrdinalIgnoreCase))
            {
                var redirect = $"/Member/Login/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            else if (ctx.Request.Path.StartsWithSegments("/Trainer", StringComparison.OrdinalIgnoreCase))
            {
                var redirect = $"/Trainer/Auth/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            else
            {
                var redirect = $"/Home/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            return Task.CompletedTask;
        };
    });

// Enables server-side session storage (used to keep small pieces of data across requests).
builder.Services.AddSession();

// Build the app – all services are now registered and ready.
var app = builder.Build();

// --- Middleware pipeline ---
// Middleware runs in order for every request. Order matters!

// In production, show a friendly error page instead of stack traces.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS tells browsers to always use HTTPS.
    app.UseHsts();
}

app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS.
app.UseStaticFiles();        // Serve static files (CSS, JS, images) from wwwroot.
app.UseRouting();            // Match incoming URLs to routes.
app.UseSession();            // Enable session state.
app.UseAuthentication();     // Identify the user from the cookie.
app.UseAuthorization();      // Check if the user has permission for the requested resource.

// --- Area routing (must come before default) ---
// Areas (Management, Member, Trainer) each have their own controllers and views.
// This route pattern handles URLs like /Management/Dashboard/Index.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Auth}/{action=Index}/{id?}");


// --- Default route (public-facing) ---
// Handles URLs for the public site, e.g. /Home/Index or just /.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();