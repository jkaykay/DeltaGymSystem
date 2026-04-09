using GymSystem.Shared.Enums;
using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

// Management controller for member subscriptions.
// A subscription links a member to a membership tier (e.g. Gold).
// Subscriptions can be filtered by state (Pending, Active, Expired) and tier name.
// Delete is Admin-only.
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class SubscriptionsController : Controller
{
    private readonly IManagementApiService _api;

    public SubscriptionsController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
        string? search = null, int? state = null, string? tierName = null,
        DateTime? startFrom = null, DateTime? startTo = null,
        string? sortBy = null, string? sortDir = null)
    {
        var result = await _api.GetSubscriptionsAsync(page, pageSize, search, state, tierName,
            startFrom, startTo, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["State"] = state;
        ViewData["TierName"] = tierName;
        ViewData["StartFrom"] = startFrom;
        ViewData["StartTo"] = startTo;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;
        ViewBag.Tiers = await _api.GetAllTiersAsync();

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var subscription = await _api.GetSubscriptionAsync(id);
        if (subscription is null)
            return NotFound();

        return View(subscription);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Members = await _api.GetAllMembersAsync();
        ViewBag.Tiers = await _api.GetAllTiersAsync();
        return View(new CreateSubscriptionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSubscriptionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Members = await _api.GetAllMembersAsync();
            ViewBag.Tiers = await _api.GetAllTiersAsync();
            return View(model);
        }

        var (success, error) = await _api.CreateSubscriptionAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create subscription. An unpaid subscription for this tier may already exist for this member.");
            ViewBag.Members = await _api.GetAllMembersAsync();
            ViewBag.Tiers = await _api.GetAllTiersAsync();
            return View(model);
        }

        TempData["Success"] = "Subscription created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var subscription = await _api.GetSubscriptionAsync(id);
        if (subscription is null)
            return NotFound();

        ViewBag.Tiers = await _api.GetAllTiersAsync();

        var vm = new EditSubscriptionViewModel
        {
            SubId = subscription.SubId,
            MemberName = subscription.MemberName,
            TierName = subscription.TierName,
            State = subscription.State,
            StartDate = subscription.StartDate == default ? null : subscription.StartDate,
            EndDate = subscription.EndDate == default ? null : subscription.EndDate
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditSubscriptionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Tiers = await _api.GetAllTiersAsync();
            return View(model);
        }

        var (success, error) = await _api.UpdateSubscriptionAsync(model.SubId, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update subscription.");
            ViewBag.Tiers = await _api.GetAllTiersAsync();
            return View(model);
        }

        TempData["Success"] = "Subscription updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteSubscriptionAsync(id);

        if (success)
            TempData["Success"] = "Subscription deleted.";
        else
            TempData["Error"] = "Failed to delete subscription.";

        return RedirectToAction(nameof(Index));
    }
}
