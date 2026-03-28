using System.Security.Claims;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Services;
using GymSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class LoginController : Controller
    {
        //this controller depends on this service
        private readonly IAuthApiService _authApiService;

        public LoginController(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        //creates an empty login model and sends it to the view (show the login page)
        [HttpGet]
        public IActionResult Index()
        {
            return View(new TrainerLoginViewModel());
        }

        //post action, when form is submitted receive the model(what user typed), or optional page to go back to
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TrainerLoginViewModel model, string? returnUrl = null)
        {
            //validation check, redisplay the page 
            if (!ModelState.IsValid)
                return View(model);

            //this converts the MVC model into shared DTO which API expects
            var loginRequest = new LoginRequest(
                model.EmailOrUserName,
                model.Password
            );

            //call the API if successful
            var loginResponse = await _authApiService.LoginAsync(loginRequest);

            //handle failed login ( if API rejects it)
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
                    //mvc app store it under the name
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
    }
}