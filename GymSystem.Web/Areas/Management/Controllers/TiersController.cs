using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

// Management controller for membership tiers (pricing plans like Gold, Silver, etc.).
// Admin-only — all actions require the Admin role.
// Provides list, details, create, edit, and delete.
[Area("Management")]
[Authorize(Roles = "Admin")]
public class TiersController : Controller
{
    private readonly IManagementApiService _api;

    public TiersController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
        string? search = null,
        string? sortBy = null, string? sortDir = null)
    {
        var tiers = await _api.GetTiersAsync(page, pageSize, search, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        return View(tiers);
    }

    public async Task<IActionResult> Details(string id)
    {
        var tier = await _api.GetTierAsync(id);
        if (tier is null)
            return NotFound();

        return View(tier);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTierViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTierViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _api.CreateTierAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create tier. The name may already be in use.");
            return View(model);
        }

        TempData["Success"] = "Tier created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var tier = await _api.GetTierAsync(id);
        if (tier is null)
            return NotFound();

        var vm = new EditTierViewModel
        {
            TierName = tier.TierName,
            Price = tier.Price
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTierViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _api.UpdateTierAsync(model.TierName, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update tier.");
            return View(model);
        }

        TempData["Success"] = "Tier updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _api.DeleteTierAsync(id);

        if (success)
            TempData["Success"] = "Tier deleted.";
        else
            TempData["Error"] = "Failed to delete tier. It may still have active subscriptions.";

        return RedirectToAction(nameof(Index));
    }
}
