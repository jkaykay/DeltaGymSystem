using GymSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers;

public class ClassesController : Controller
{
    private readonly IMemberApiService _api;

    public ClassesController(IMemberApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var classes = await _api.GetClassesAsync();
        ViewBag.Classes = classes.Items;

        return View();
    }
}