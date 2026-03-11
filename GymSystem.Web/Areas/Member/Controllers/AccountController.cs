using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers
{
    [Area("Member")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IManagementApiService _api;

        public AccountController(IManagementApiService api)
        {
            _api = api;
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
        public async Task<IActionResult> Login(MemberLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var response = await _api.LoginAsync(model.EmailOrUserName, model.Password);

            if (response is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View(model);
            }

            // Verify the user has a Member role
            if (response.Roles.Contains("Admin") || response.Roles.Contains("Staff") || response.Roles.Contains("Trainer"))
            {
                ModelState.AddModelError(string.Empty, "Please login into the correct portal.");
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

            return RedirectToAction("Index", "Dashboard", new { area = "Member" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
