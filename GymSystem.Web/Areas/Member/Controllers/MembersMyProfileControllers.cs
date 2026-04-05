using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers;

[Authorize]
[Area("Member")]
public class MyProfileController : Controller
{
    private readonly IMemberApiService _memberApiService;

    public MyProfileController(IMemberApiService memberApiService)
    {
        _memberApiService = memberApiService;
    }

    public async Task<IActionResult> Index()
    {

        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = await _memberApiService.GetMyProfileAsync();

        if (profile == null)
        {
            ModelState.AddModelError(string.Empty, "Could not load profile.");
            return View(new ProfileViewModel());
        }

        var qrCode = await _memberApiService.GetMyQRAsync(memberId!);

        var model = new ProfileViewModel
        {
            UserId          = profile.Id,
            UserName        = profile.UserName,
            FullName        = $"{profile.FirstName} {profile.LastName}",
            Email           = profile.Email,
            FirstName       = profile.FirstName,
            LastName        = profile.LastName,
            MembershipName  = profile.MembershipName,
            MembershipPrice = profile.MembershipPrice,
            QrCodeBase64    = qrCode != null ? qrCode.QrCodeBase64 : null
        };

        return View(model);
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
                ModelState.AddModelError(string.Empty, result.Error ?? "Update failed.");
                return View("Index", model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Something went wrong: " + ex.Message);
            return View("Index", model);
        }
    }
}