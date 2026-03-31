using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Services;

namespace GymSystem.Web.Areas.Member.Controllers
{
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
            return View();
        }

      
        // POST: Register User
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 1. Validate ViewModel (includes ConfirmPassword)
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
                    return RedirectToAction("Login", "Account", new { area = "Member" });
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