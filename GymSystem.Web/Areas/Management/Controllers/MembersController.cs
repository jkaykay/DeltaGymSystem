using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class MembersController : Controller
{
    private readonly IManagementApiService _api;

    public MembersController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var result = await _api.GetMembersAsync(page, pageSize);
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
            Active = member.Active
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditMemberViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

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