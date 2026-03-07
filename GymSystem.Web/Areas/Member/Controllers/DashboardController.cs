using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using GymSystem.Web.Areas.Member.Models;

namespace GymSystem.Web.Areas.Member.Controllers;

[Area("Member")]
[Authorize(Roles = "Member")]
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