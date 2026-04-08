using GymSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers;

// Public-facing controller for the Classes listing page.
// Allows visitors (no login required) to browse available gym classes.
public class ClassesController : Controller
{
    private readonly IMemberApiService _api;

    public ClassesController(IMemberApiService api)
    {
        _api = api;
    }

    // GET /Classes — Fetches all classes from the API and displays them.
    public async Task<IActionResult> Index()
    {
        var classes = await _api.GetClassesAsync();
        ViewBag.Classes = classes.Items;

        return View();
    }
}
