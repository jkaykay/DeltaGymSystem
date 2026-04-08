using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Services;

namespace GymSystem.Web.Controllers;

// Public-facing controller for the Memberships page.
// Displays available membership tiers (pricing plans) and hosts
// the "Ask Delta" AI chat feature where visitors can ask questions.
public class MembershipsController : Controller
{
    private readonly IMemberApiService _api;

    public MembershipsController(IMemberApiService api)
    {
        _api = api;
    }

    // GET /Memberships — Fetches all tiers and displays them.
    public async Task<IActionResult> Index()
    {
        var tiers = await _api.GetAllTiersAsync();
        ViewBag.Tiers = tiers;

        return View();
    }

    // POST /Memberships/AskDelta — Sends the user's question to the AI chatbot
    // and displays the answer on the same Memberships page.
    // ValidateAntiForgeryToken prevents CSRF attacks.
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
