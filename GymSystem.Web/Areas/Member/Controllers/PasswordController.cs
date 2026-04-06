using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Member.Controllers;

[Area("Member")]
[Authorize(Roles = "Member")]
public class PasswordController : Controller
{
    private readonly IAuthApiService _authApi;

    public PasswordController(IAuthApiService authApi)
    {
        _authApi = authApi;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, error) = await _authApi.ChangePasswordAsync(model.CurrentPassword, model.NewPassword);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Password change failed. Please check your current password and try again.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Password changed successfully!";
        return RedirectToAction("Index");
    }
}
