using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;

namespace GymSystem.Web.Areas.Member.Controllers
{
    [Authorize]
    [Area("Member")]
    public class MyProfileController : Controller
    {
        private readonly IMemberApiService _memberApiService;

        public MyProfileController(IMemberApiService memberApiService)
        {
            _memberApiService = memberApiService;
        }

        // GET: Member/MyProfile
        public async Task<IActionResult> Index()
        {
            try
            {
                var profile = await _memberApiService.GetMyProfileAsync();

                if (profile == null)
                    return RedirectToAction("Index", "Login", new { area = "Member" });

                var model = new ProfileViewModel
                  {
                    UserName = profile.UserName,
                    FullName = profile.FirstName + " " + profile.LastName,
                    Email = profile.Email,
                    Telephone = profile.Telephone,
                    EmergencyContact = profile.EmergencyContact,
                    Weight = profile.Weight,
                    MembershipName = profile.MembershipName,
                    MembershipPrice = profile.MembershipPrice,
                    MemberCode = profile.MemberCode
                };

                return View(model);
            }

            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while loading your profile. Please try again later.");
                return View(new ProfileViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Index", model);

                var result = await _memberApiService.UpdateProfileAsync(model);
                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Error ?? "Update failed.");
                    return View("Index", model);
                }

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong: " + ex.Message);
                return View("Index", model);
            }
        }
    }
}