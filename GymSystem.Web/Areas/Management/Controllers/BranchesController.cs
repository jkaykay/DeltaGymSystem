using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

// Management controller for gym branches (physical locations).
// Provides list (with paging/search/sort), details, create, edit, and delete.
// Create, Edit, and Delete are restricted to Admin-only.
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class BranchesController : Controller
{
    private readonly IManagementApiService _api;

    public BranchesController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
        string? search = null,
        string? sortBy = null, string? sortDir = null)
    {
        var branches = await _api.GetBranchesAsync(page, pageSize, search, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        return View(branches);
    }

    public async Task<IActionResult> Details(int id)
    {
        var branch = await _api.GetBranchAsync(id);
        if (branch is null)
            return NotFound();

        return View(branch);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new CreateBranchViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateBranchViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, error) = await _api.CreateBranchAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create branch.");
            return View(model);
        }

        TempData["Success"] = "Branch created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var branch = await _api.GetBranchAsync(id);
        if (branch is null)
            return NotFound();

        var vm = new EditBranchViewModel
        {
            BranchId = branch.BranchId,
            Address = branch.Address,
            City = branch.City,
            Province = branch.Province,
            PostCode = branch.PostCode
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(EditBranchViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, error) = await _api.UpdateBranchAsync(model.BranchId, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update branch.");
            return View(model);
        }

        TempData["Success"] = "Branch updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteBranchAsync(id);

        if (success)
            TempData["Success"] = "Branch deleted.";
        else
            TempData["Error"] = "Failed to delete branch.";

        return RedirectToAction(nameof(Index));
    }
}
