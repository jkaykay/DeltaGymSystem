using System.Security.Claims;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Services;
using GymSystem.Web.Areas.Trainer.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    [AllowAnonymous]
    public class AuthController : Controller
    {
        
        private readonly IAuthApiService _authApiService;

        public AuthController(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Trainer") || User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Trainer" });

                if (User.IsInRole("Member"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Member" });

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(new TrainerLoginViewModel());
        }

        //login method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TrainerLoginViewModel model, string? returnUrl = null)
        {
           
            if (!ModelState.IsValid)
                return View(model);

            
            var loginRequest = new LoginRequest(
                model.EmailOrUserName,
                model.Password
            );

           
            var loginResponse = await _authApiService.LoginAsync(loginRequest);

            
            if (loginResponse is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email/username or password.");
                return View(model);
            }

            //create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResponse.Id),
                new Claim("userName", loginResponse.UserName),
                new Claim("email", loginResponse.Email),
                new Claim("firstName", loginResponse.FirstName),
                new Claim("lastName", loginResponse.LastName),
                new Claim("gymLocation", loginResponse.GymLocation)
            };

            //add role to the user 
            foreach (var role in loginResponse.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //create cookie identity
            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            //remember me, the login cookied presists longer

            var authProperties = new AuthenticationProperties
            {

                IsPersistent = model.RememberMe
            };

            // store the JWT token 
            authProperties.StoreTokens(new[]
            {
                new AuthenticationToken
                {
                    
                    Name = "access_token",
                    Value = loginResponse.Token
                }
            });

            //sign the user in, and creates the authentication cookie
            await HttpContext.SignInAsync("Cookies", principal, authProperties);

            //redirect to dashboard, after loginp
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Trainer" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                await _authApiService.LogoutAsync(token);
            }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("Cookies");

            return RedirectToAction("Index", "Auth", new { area = "Trainer" });

        }

        public IActionResult AccessDenied() => View();
    }
}