using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class AttendancesController : Controller
{
    private readonly IManagementApiService _api;

    public AttendancesController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAttendancesAsync();
        return View(result);
    }

    public async Task<IActionResult> Active()
    {
        var result = await _api.GetActiveAttendancesAsync();
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var attendance = await _api.GetAttendanceAsync(id);
        if (attendance is null)
            return NotFound();

        return View(attendance);
    }

    [HttpGet]
    public async Task<IActionResult> CheckIn()
    {
        ViewBag.Members = await _api.GetAllMembersAsync();
        return View(new CheckInViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Members = await _api.GetAllMembersAsync();
            return View(model);
        }

        var success = await _api.CheckInMemberAsync(model.UserId);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to check in member. They may already have an active session or their account is inactive.");
            ViewBag.Members = await _api.GetAllMembersAsync();
            return View(model);
        }

        TempData["Success"] = "Member checked in successfully.";
        return RedirectToAction(nameof(Active));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(string memberId)
    {
        var success = await _api.CheckOutMemberAsync(memberId);

        if (success)
            TempData["Success"] = "Member checked out successfully.";
        else
            TempData["Error"] = "Failed to check out member. They may not have an active session.";

        return RedirectToAction(nameof(Active));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteAttendanceAsync(id);

        if (success)
            TempData["Success"] = "Attendance record deleted.";
        else
            TempData["Error"] = "Failed to delete attendance record.";

        return RedirectToAction(nameof(Index));
    }
}