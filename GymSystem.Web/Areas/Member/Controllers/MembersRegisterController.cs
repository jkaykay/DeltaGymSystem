using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Services;

namespace GymSystem.Web.Areas.Member.Controllers
{
    // Handles new member self-registration.
    // GET shows the registration form; POST validates the form data,
    // maps it to a DTO, and calls the backend auth/register API.
    // On success, the user is redirected to the login page.
    [Area("Member")]
    public class RegisterController : Controller
    {
        private readonly IMemberApiService _memberApiService;

        public RegisterController(IMemberApiService memberApiService)
        {
            _memberApiService = memberApiService;
        }

      
        // GET: Register Page

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            
                return RedirectToAction("Index", "Dashboard", new { area = "Member" });
            

            return View();
        }

      
        // POST: Register User
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
                if (User.Identity?.IsAuthenticated == true)            
                return RedirectToAction("Index", "Dashboard", new { area = "Member" });
            
            //Validate ViewModel (includes ConfirmPassword)
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // 2. Map ViewModel → DTO
                var dto = new RegisterRequest(
                    model.UserName,
                    model.Email,
                    model.Password,
                    model.FirstName,
                    model.LastName
                );

                // 3. Call API
                var result = await _memberApiService.RegisterAsync(dto);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction("Index", "Login", new { area = "Member" });
                }

                // 4. Show API error
                ModelState.AddModelError("", result.Error ?? "Registration failed.");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong: " + ex.Message);
                return View("Index", model);
            }
        }
    }
}
