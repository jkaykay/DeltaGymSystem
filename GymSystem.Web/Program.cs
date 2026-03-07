using GymSystem.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// --- MVC with Area support ---
builder.Services.AddControllersWithViews();

// --- Required for TokenDelegatingHandler ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenDelegatingHandler>();

// --- API service ---
builder.Services.AddScoped<IManagementApiService, ManagementApiService>();

// --- Typed HttpClient for API consumption ---
builder.Services.AddHttpClient("GymApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["GymApi:BaseUrl"]
        ?? throw new InvalidOperationException("GymApi:BaseUrl is not configured."));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
    .AddHttpMessageHandler<TokenDelegatingHandler>();

// --- Authentication (cookie-based, tokens stored in session/cookie) ---
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Events.OnRedirectToLogin = ctx =>
        {
            var returnUrl = ctx.Request.Path + ctx.Request.QueryString;
            if (ctx.Request.Path.StartsWithSegments("/Management", StringComparison.OrdinalIgnoreCase))
            {
                var redirect = $"/Management/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            else
            {
                var redirect = $"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            var returnUrl = ctx.Request.Path + ctx.Request.QueryString;
            if (ctx.Request.Path.StartsWithSegments("/Management", StringComparison.OrdinalIgnoreCase))
            {
                var redirect = $"/Management/Account/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            else
            {
                var redirect = $"/Account/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}";
                ctx.Response.Redirect(redirect);
            }
            return Task.CompletedTask;
        };
    });

builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// --- Area routing (must come before default) ---
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

// --- Default route (public-facing) ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();