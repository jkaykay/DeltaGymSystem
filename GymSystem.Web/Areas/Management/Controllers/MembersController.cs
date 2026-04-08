using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

// Management controller for gym members.
// Provides CRUD operations: list (with paging/filtering), details, create, edit,
// toggle active/inactive, and delete.
// Only Admin and Staff roles can access this controller.
// Phone numbers are stored with a +44 UK prefix in the database but displayed
// without it in the UI, using the helper methods below.
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class MembersController : Controller
{
    private readonly IManagementApiService _api;

    public MembersController(IManagementApiService api)
    {
        _api = api;
    }

    // Removes the +44 UK prefix for display in the edit form.
    private static string? StripUkPrefix(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Replace(" ", "");
        if (phone.StartsWith("+44")) phone = phone[3..];
        return phone;
    }

    // Adds back the +44 UK prefix before saving to the API.
    private static string? PrependUkPrefix(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Replace(" ", "").TrimStart('0');
        return "+44" + phone;
    }

    // GET /Management/Members — Lists members with paging, search, and filtering.
    // Filter values are stored in ViewData so the view can re-populate the filter form.
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
    string? search = null, bool? active = null,
    DateTime? joinedFrom = null, DateTime? joinedTo = null,
    string? sortBy = null, string? sortDir = null)
    {
        var result = await _api.GetMembersAsync(page, pageSize, search, active,
            joinedFrom, joinedTo, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["Active"] = active;
        ViewData["JoinedFrom"] = joinedFrom;
        ViewData["JoinedTo"] = joinedTo;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        return View(result);
    }

    public async Task<IActionResult> Details(string id)
    {
        var member = await _api.GetMemberAsync(id);
        if (member is null)
            return NotFound();

        return View(member);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var member = await _api.GetMemberAsync(id);
        if (member is null)
            return NotFound();

        var vm = new EditMemberViewModel
        {
            Id = member.Id,
            Email = member.Email,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Active = member.Active,
            PhoneNumber = StripUkPrefix(member.PhoneNumber)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditMemberViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.PhoneNumber = PrependUkPrefix(model.PhoneNumber);

        var success = await _api.UpdateMemberAsync(model.Id, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update member.");
            return View(model);
        }

        TempData["Success"] = "Member updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        await _api.ToggleMemberActiveAsync(id);
        TempData["Success"] = "Member status updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _api.DeleteMemberAsync(id);
        TempData["Success"] = "Member deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new CreateMemberViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateMemberViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.PhoneNumber = PrependUkPrefix(model.PhoneNumber);

        var success = await _api.CreateMemberAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create member. Check password requirements.");
            return View(model);
        }

        TempData["Success"] = "Member created successfully.";
        return RedirectToAction(nameof(Index));
    }
}
