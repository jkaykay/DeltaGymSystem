using System.Security.Claims;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Trainer.Models;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class ProfileController : Controller
    {
        private readonly IAuthApiService _authApiService;

        public ProfileController(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        [HttpGet]
        public IActionResult Index(bool edit = false)
        {
            var firstName = User.FindFirst("firstName")?.Value ?? "";
            var lastName = User.FindFirst("lastName")?.Value ?? "";
            var email = User.FindFirst("email")?.Value ?? "";
            var userName = User.FindFirst("userName")?.Value ?? "";
            var gymLocation = User.FindFirst("gymLocation")?.Value ?? "";

            var model = new TrainerProfileViewModel
            {
                FullName = $"{firstName} {lastName}".Trim(),
                RoleLabel = "Trainer",
                UserName = userName,
                Email = email,
                GymLocation = gymLocation,
                IsEditing = edit
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TrainerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.IsEditing = true;
                return View(model);
            }

            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                model.IsEditing = true;
                return View(model);
            }

            var request = new UpdateProfileRequest(model.Email);

            var success = await _authApiService.UpdateProfileAsync(request, token);

            if (!success)
            {
                model.IsEditing = true;
                ModelState.AddModelError(string.Empty, "Could not save changes.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ""),
                new Claim("userName", User.FindFirst("userName")?.Value ?? ""),
                new Claim("email", model.Email),
                new Claim("firstName", User.FindFirst("firstName")?.Value ?? ""),
                new Claim("lastName", User.FindFirst("lastName")?.Value ?? ""),
                new Claim("gymLocation", User.FindFirst("gymLocation")?.Value ?? "")
            };

            foreach (var role in User.FindAll(ClaimTypes.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Value));
            }

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);



            var authProperties = new AuthenticationProperties();

            authProperties.StoreTokens(new[] {

                new AuthenticationToken{
                    Name = "access_token",
                    Value = token
                }
            });

            await HttpContext.SignInAsync("Cookies", principal, authProperties);

            return RedirectToAction("Index");
        }
    }
}