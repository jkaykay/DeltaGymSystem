using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Management.ViewModels;

namespace GymSystem.Web.Areas.Management.Controllers;

/// <summary>
/// Dashboard for the Management area (admin panel home page).
/// [Authorize(Roles = "Admin,Staff")] means only users with Admin or Staff
/// role can access this page; everyone else gets redirected to login.
/// Displays summary statistics (total members, staff, trainers) and recent sign-ups.
/// </summary>
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class DashboardController : Controller
{
    private readonly IManagementApiService _api;

    public DashboardController(IManagementApiService api)
    {
        _api = api;
    }

    /// <summary>
    /// GET /Management/Dashboard — Fetches totals and recent sign-ups
    /// and passes them to the dashboard view.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalMembers = (await _api.GetTotalMembersAsync()).Count,
            TotalStaff = (await _api.GetTotalStaffAsync()).Count,
            TotalTrainers = (await _api.GetTotalTrainersAsync()).Count,
            RecentSignups = await _api.GetRecentSignupsAsync()
        };

        return View(model);
    }
}