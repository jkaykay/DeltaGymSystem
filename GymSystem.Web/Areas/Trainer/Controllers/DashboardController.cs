using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using GymSystem.Web.Areas.Trainer.Models;

namespace GymSystem.Web.Areas.Trainer.Controllers;

[Area("Trainer")]
[Authorize(Roles = "Trainer")]
public class DashboardController : Controller
{
    private readonly HttpClient _api;

    public DashboardController(IHttpClientFactory httpClientFactory)
    {
        _api = httpClientFactory.CreateClient("GymApi");
    }

    public async Task<IActionResult> Index()
    {
        // Call API — no business logic here
        return View();
    }
}