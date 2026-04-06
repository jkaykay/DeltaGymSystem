using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Services;

namespace GymSystem.Web.Controllers;

public class MembershipsController : Controller
{
    private readonly IMemberApiService _api;

    public MembershipsController(IMemberApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var tiers = await _api.GetAllTiersAsync();
        ViewBag.Tiers = tiers;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AskDelta(string prompt)
    {
        var tiers = await _api.GetAllTiersAsync();
        ViewBag.Tiers = tiers;

        if (string.IsNullOrWhiteSpace(prompt))
        {
            ViewBag.DeltaAnswer = "Please type a question.";
            return View("Index");
        }

        var answer = await _api.AskDeltaAsync(prompt);
        ViewBag.DeltaAnswer = answer ?? "Sorry, I couldn't get an answer right now. Please try again later.";
        ViewBag.DeltaPrompt = prompt;

        return View("Index");
    }
}