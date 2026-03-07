using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Management.ViewModels;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class DashboardController : Controller
{
    private readonly IManagementApiService _api;

    public DashboardController(IManagementApiService api)
    {
        _api = api;
    }
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalMembers = (await _api.GetTotalMembersAsync()).Count,
            TotalStaff = (await _api.GetTotalStaffAsync()).Count,
            RecentSignups = await _api.GetRecentSignupsAsync()
        };

        return View(model);
    }
}