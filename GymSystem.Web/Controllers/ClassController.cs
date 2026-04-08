using GymSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers;

/// <summary>
/// Public-facing controller for the Classes listing page.
/// Allows visitors (no login required) to browse available gym classes.
/// </summary>
public class ClassesController : Controller
{
    private readonly IMemberApiService _api;

    public ClassesController(IMemberApiService api)
    {
        _api = api;
    }

    /// <summary>
    /// GET /Classes — Fetches all classes from the API and displays them.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var classes = await _api.GetClassesAsync();
        ViewBag.Classes = classes.Items;

        return View();
    }
}