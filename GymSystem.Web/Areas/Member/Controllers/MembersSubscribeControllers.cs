using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers;

[Authorize(Roles = "Member")]
[Area("Member")]
public class SubscribeController : Controller
{
    private readonly IMemberApiService _api;

    public SubscribeController(IMemberApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? tier)
    {
        var tiers = await _api.GetAllTiersAsync();
        var selected = tiers.FirstOrDefault(t =>
            string.Equals(t.TierName, tier, StringComparison.OrdinalIgnoreCase))
            ?? tiers.FirstOrDefault();

        if (selected is null)
        {
            TempData["ErrorMessage"] = "No membership plans available.";
            return RedirectToAction("Index", "Memberships", new { area = "" });
        }

        var firstName = User.FindFirstValue(ClaimTypes.GivenName) ?? "";
        var lastName = User.FindFirstValue(ClaimTypes.Surname) ?? "";

        var model = new SubscribeViewModel
        {
            MemberName = $"{firstName} {lastName}".Trim(),
            TierName = selected.TierName,
            Price = selected.Price
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(string tierName, decimal amount)
    {
        var subResult = await _api.CreateMySubscriptionAsync(tierName);

        if (!subResult.Success || subResult.Data is null)
        {
            TempData["ErrorMessage"] = subResult.Error ?? "Subscription creation failed.";
            return RedirectToAction("Index", new { tier = tierName });
        }

        var payResult = await _api.CreateMyPaymentAsync(subResult.Data.SubId, amount);

        if (!payResult.Success)
        {
            TempData["ErrorMessage"] = payResult.Error ?? "Payment failed.";
            return RedirectToAction("Index", new { tier = tierName });
        }

        TempData["SuccessMessage"] = "Payment confirmed! Your membership is now active.";
        return RedirectToAction("Index", new { tier = tierName });
    }
}