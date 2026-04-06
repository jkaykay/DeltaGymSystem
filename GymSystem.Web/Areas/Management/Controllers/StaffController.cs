using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class StaffController : Controller
{
    private readonly IManagementApiService _api;

    public StaffController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
    string? search = null,
    DateTime? hiredFrom = null, DateTime? hiredTo = null,
    string? sortBy = null, string? sortDir = null)
    {
        var result = await _api.GetStaffAsync(page, pageSize, search,
            hiredFrom, hiredTo, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["HiredFrom"] = hiredFrom;
        ViewData["HiredTo"] = hiredTo;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        return View(result);
    }

    public async Task<IActionResult> Details(string id)
    {
        var staff = await _api.GetStaffMemberAsync(id);
        if (staff is null)
            return NotFound();

        return View(staff);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new CreateStaffViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _api.CreateStaffAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create staff member. Check password requirements.");
            return View(model);
        }

        TempData["Success"] = "Staff member created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        var staff = await _api.GetStaffMemberAsync(id);
        if (staff is null)
            return NotFound();

        ViewBag.Branches = await _api.GetAllBranchesAsync();

        var vm = new EditStaffViewModel
        {
            Id = staff.Id,
            Email = staff.Email,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            EmployeeId = staff.EmployeeId,
            BranchId = staff.BranchId,
            PhoneNumber = staff.PhoneNumber
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(EditStaffViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        var success = await _api.UpdateStaffAsync(model.Id, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update staff member.");
            ViewBag.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        TempData["Success"] = "Staff member updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _api.DeleteStaffAsync(id);

        if (success)
            TempData["Success"] = "Staff member deleted.";
        else
            TempData["Error"] = "Failed to delete staff member.";

        return RedirectToAction(nameof(Index));
    }
}