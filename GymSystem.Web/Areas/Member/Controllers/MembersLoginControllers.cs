using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers;

[Area("Member")]
[AllowAnonymous]
public class LoginController : Controller
{
    private readonly IAuthApiService _authApi;

    public LoginController(IAuthApiService authApi)
    {
        _authApi = authApi;
    }

    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard", new { area = "Member" });

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MemberLoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var response = await _authApi.LoginAsync(new LoginRequest(model.EmailOrUserName, model.Password));

        if (response is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        if (response.Roles.Contains("Admin") || response.Roles.Contains("Staff") || response.Roles.Contains("Trainer"))
        {
            ModelState.AddModelError(string.Empty, "Please login into the correct portal.");
            return View(model);
        }

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

        var properties = new AuthenticationProperties { IsPersistent = model.RememberMe };
        properties.StoreTokens(
        [
            new AuthenticationToken { Name = "access_token", Value = response.Token }
        ]);

        await HttpContext.SignInAsync("Cookies", principal, properties);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard", new { area = "Member" });
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
        return RedirectToAction("Index");
    }

    public IActionResult AccessDenied() => View();
}