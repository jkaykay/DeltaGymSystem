using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using GymSystem.Shared.DTOs;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers
{
    [Authorize(Roles = "Member")]
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

                QRCodeResponse? qr = null;
                if (profile.Active)
                {
                    try { qr = await _memberApiService.GetMyQRAsync(profile.Id); } catch { }
                }

                var model = new ProfileViewModel
                {
                    Id = profile.Id,
                    UserName = profile.UserName,
                    Email = profile.Email,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    JoinDate = profile.JoinDate,
                    Active = profile.Active,
                    QrCodeBase64 = qr?.QrCodeBase64,
                    QrExpiresAt = qr?.ExpiresAt
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

                var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(memberId))
                    return RedirectToAction("Index", "Login", new { area = "Member" });

                var request = new UpdateMemberRequest(
                    Email: model.Email,
                    FirstName: model.FirstName,
                    LastName: model.LastName
                );

                var result = await _memberApiService.UpdateProfileAsync(memberId, request);
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