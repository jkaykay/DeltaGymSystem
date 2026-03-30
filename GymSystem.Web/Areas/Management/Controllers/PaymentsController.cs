using GymSystem.Shared.Enums;
using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class PaymentsController : Controller
{
    private readonly IManagementApiService _api;

    public PaymentsController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var result = await _api.GetPaymentsAsync(page, pageSize);
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var payment = await _api.GetPaymentAsync(id);
        if (payment is null)
            return NotFound();

        return View(payment);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var allSubscriptions = await _api.GetAllSubscriptionsAsync();
        ViewBag.PendingSubscriptions = allSubscriptions
            .Where(s => s.State == SubscriptionState.Pending)
            .ToList();

        return View(new CreatePaymentViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var allSubscriptions = await _api.GetAllSubscriptionsAsync();
            ViewBag.PendingSubscriptions = allSubscriptions
                .Where(s => s.State == SubscriptionState.Pending)
                .ToList();
            return View(model);
        }

        var success = await _api.CreatePaymentAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to record payment. Ensure the amount matches the tier price exactly.");
            var allSubscriptions = await _api.GetAllSubscriptionsAsync();
            ViewBag.PendingSubscriptions = allSubscriptions
                .Where(s => s.State == SubscriptionState.Pending)
                .ToList();
            return View(model);
        }

        TempData["Success"] = "Payment recorded successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeletePaymentAsync(id);

        if (success)
            TempData["Success"] = "Payment deleted.";
        else
            TempData["Error"] = "Failed to delete payment.";

        return RedirectToAction(nameof(Index));
    }
}