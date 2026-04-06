using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
public class AccountController : Controller
{
    private readonly IAuthApiService _authApi;

    public AccountController(IAuthApiService authApi)
    {
        _authApi = authApi;
    }

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

    public IActionResult AccessDenied()
    {
        return View();
    }
}