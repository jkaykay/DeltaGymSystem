using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

// Management controller for staff members.
// Provides list (with paging/filtering), details, create, edit, and delete.
// Create, Edit, and Delete require the Admin role specifically.
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class StaffController : Controller
{
    private readonly IManagementApiService _api;

    public StaffController(IManagementApiService api)
    {
        _api = api;
    }

    private static string? StripUkPrefix(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Replace(" ", "");
        if (phone.StartsWith("+44")) phone = phone[3..];
        return phone;
    }

    private static string? PrependUkPrefix(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Replace(" ", "").TrimStart('0');
        return "+44" + phone;
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
    public async Task<IActionResult> Create()
    {
        var model = new CreateStaffViewModel
        {
            Branches = await _api.GetAllBranchesAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        model.PhoneNumber = PrependUkPrefix(model.PhoneNumber);

        var (success, error) = await _api.CreateStaffAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create staff member. Check password requirements.");
            model.Branches = await _api.GetAllBranchesAsync();
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

        var vm = new EditStaffViewModel
        {
            Id = staff.Id,
            Email = staff.Email,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            EmployeeId = staff.EmployeeId,
            BranchId = staff.BranchId,
            PhoneNumber = StripUkPrefix(staff.PhoneNumber),
            Branches = await _api.GetAllBranchesAsync()
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
            model.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        model.PhoneNumber = PrependUkPrefix(model.PhoneNumber);

        var (success, error) = await _api.UpdateStaffAsync(model.Id, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update staff member.");
            model.Branches = await _api.GetAllBranchesAsync();
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
