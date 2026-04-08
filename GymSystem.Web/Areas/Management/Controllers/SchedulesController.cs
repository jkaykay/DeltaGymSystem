using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

/// <summary>
/// Management controller for employee work schedules.
/// An admin assigns a schedule (start/end time) to a staff member or trainer.
/// The EmployeesByBranch endpoint is an AJAX helper used by the Create form
/// to populate the employee dropdown when a branch is selected.
/// </summary>
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class SchedulesController : Controller
{
    private readonly IManagementApiService _api;

    public SchedulesController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
        string? search = null,
        DateTime? dateFrom = null, DateTime? dateTo = null,
        string? sortBy = null, string? sortDir = null)
    {
        var schedules = await _api.GetSchedulesAsync(page, pageSize, search,
            dateFrom, dateTo, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["DateFrom"] = dateFrom;
        ViewData["DateTo"] = dateTo;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        return View(schedules);
    }

    public async Task<IActionResult> Details(int id)
    {
        var schedule = await _api.GetScheduleAsync(id);
        if (schedule is null)
            return NotFound();

        return View(schedule);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Branches = await _api.GetAllBranchesAsync();
        return View(new CreateScheduleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateScheduleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        var success = await _api.CreateScheduleAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create schedule. There may be a time conflict.");
            ViewBag.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        TempData["Success"] = "Schedule created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EmployeesByBranch(int branchId)
    {
        var employees = await _api.GetEmployeesByBranchAsync(branchId);
        return Json(employees.Select(e => new
        {
            e.Id,
            Name = $"{e.FirstName} {e.LastName} ({string.Join(", ", e.Roles)})"
        }));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var schedule = await _api.GetScheduleAsync(id);
        if (schedule is null)
            return NotFound();

        var vm = new EditScheduleViewModel
        {
            ScheduleId = schedule.ScheduleId,
            UserId = schedule.UserId,
            UserName = schedule.UserName,
            Start = schedule.Start,
            End = schedule.End
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(EditScheduleViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _api.UpdateScheduleAsync(model.ScheduleId, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update schedule. There may be a time conflict.");
            return View(model);
        }

        TempData["Success"] = "Schedule updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteScheduleAsync(id);

        if (success)
            TempData["Success"] = "Schedule deleted.";
        else
            TempData["Error"] = "Failed to delete schedule.";

        return RedirectToAction(nameof(Index));
    }
}
