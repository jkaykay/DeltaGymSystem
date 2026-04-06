using System.Security.Claims;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Areas.Trainer.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    [Authorize(Roles = "Trainer,Admin")]
    public class ProfileController : Controller
    {
        private readonly ITrainerApiService _trainerApiService;


        public ProfileController(ITrainerApiService trainerApiService)
        {
            _trainerApiService = trainerApiService;
        }

        //load profile page
        [HttpGet]
        public async Task<IActionResult> Index(bool edit = false)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                return Challenge();

            var trainer = await _trainerApiService.GetTrainerProfileAsync(token);

            if (trainer == null)
            {
                return View(new TrainerProfileViewModel
                { 
                    RoleLabel = "Trainer",
                    IsEditing = edit
                });
            }

            var gymLocation = User.FindFirstValue("gymLocation") ?? "";

            var model = new TrainerProfileViewModel
            {
                FullName = $"{trainer.FirstName} {trainer.LastName}".Trim(),
                RoleLabel = "Trainer",
                UserName = trainer.UserName ?? "",
                Email = trainer.Email ?? "",
                PhoneNumber = trainer.PhoneNumber,
                GymLocation = gymLocation,
                IsEditing = edit

            };

            return View(model);
        }

        //helper method if post action fails making sure the fields are not empty
        private async Task FillProfileDisplayDataAsync(TrainerProfileViewModel model, string token)
        {
            var trainer = await _trainerApiService.GetTrainerProfileAsync(token);

            model.FullName = $"{trainer?.FirstName} {trainer?.LastName}".Trim();
            model.UserName = trainer?.UserName ?? "";
            model.GymLocation = User.FindFirstValue("gymLocation") ?? "";
            model.RoleLabel = "Trainer";
            model.IsEditing = true;
            
        }

        //update trainer profile after save changes
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

            var request = new UpdateTrainerProfileRequest(
                Email: model.Email,
                FirstName: null,
                LastName: null,
                PhoneNumber: model.PhoneNumber
                );

            var success = await _trainerApiService.UpdateTrainerProfileAsync(request, token);

            if (!success)
            {
                await FillProfileDisplayDataAsync(model, token);
                ModelState.AddModelError(string.Empty, "Could not save changes.");
                return View(model);
            }


            return RedirectToAction("Index");
        }
    }
}