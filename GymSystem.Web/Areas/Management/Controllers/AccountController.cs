using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Management.Controllers;

/// <summary>
/// Handles login, logout, and access-denied for the Management area.
/// [Area("Management")] tells the routing system this controller lives
/// under the /Management URL prefix.
/// Unlike Member/Trainer login, this controller verifies the user has
/// an Admin or Staff role before granting access.
/// </summary>
[Area("Management")]
public class AccountController : Controller
{
    // Service used to call the backend auth API (login, logout, etc.).
    private readonly IAuthApiService _authApi;

    public AccountController(IAuthApiService authApi)
    {
        _authApi = authApi;
    }

    /// <summary>
    /// GET /Management/Account/Login — Shows the login form.
    /// If the user is already authenticated, redirects straight to the dashboard.
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// POST /Management/Account/Login — Processes the submitted login form.
    /// Steps:
    ///   1. Validate the form fields.
    ///   2. Call the auth API to verify credentials.
    ///   3. Check the user has Admin or Staff role.
    ///   4. Create claims (identity info) and store the JWT in the auth cookie.
    ///   5. Sign the user in and redirect to the dashboard (or returnUrl).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(ManagementLoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var response = await _authApi.LoginAsync(new LoginRequest(model.Email, model.Password));

        if (response is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // Verify the user has a management role
        if (!response.Roles.Contains("Admin") && !response.Roles.Contains("Staff"))
        {
            ModelState.AddModelError(string.Empty, "You do not have access to the management portal.");
            return View(model);
        }

        // Build claims from the API response
        // Claims are key-value pairs that describe the user (ID, email, name, roles).
        // They are stored inside the authentication cookie.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, response.Id),
            new(ClaimTypes.Email, response.Email),
            new(ClaimTypes.GivenName, response.FirstName),
            new(ClaimTypes.Surname, response.LastName),
            new(ClaimTypes.Name, response.Email)
        };

        foreach (var role in response.Roles)
            claims.Add(new(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        // Store the JWT so TokenDelegatingHandler can attach it to API calls
        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe
        };
        properties.StoreTokens(
        [
            new AuthenticationToken { Name = "access_token", Value = response.Token }
        ]);

        await HttpContext.SignInAsync("Cookies", principal, properties);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard", new { area = "Management" });
    }

    /// <summary>
    /// POST /Management/Account/Logout — Signs the user out.
    /// First tells the API to invalidate the JWT, then clears the local cookie.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var token = await HttpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrWhiteSpace(token))
        {
            await _authApi.LogoutAsync(token);
        }

        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Login");
    }

    /// <summary>Shows the "Access Denied" page when a user lacks the required role.</summary>
    public IActionResult AccessDenied()
    {
        return View();
    }
}