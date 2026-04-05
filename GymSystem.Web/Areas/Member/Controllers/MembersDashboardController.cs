using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers;

[Authorize]
[Area("Member")]
public class DashboardController : Controller
{
    private readonly IMemberApiService _api;

    public DashboardController(IMemberApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var firstName = User.FindFirstValue(ClaimTypes.GivenName) ?? "Member";

        var bookings = await _api.GetMyBookingsAsync();
        var attendances = await _api.GetMyAttendancesAsync(memberId);
        var payments = await _api.GetMyPaymentsAsync();

        var now = DateTime.UtcNow;
        var upcomingCount = bookings.Items.Count(b => b.SessionStart > now);

        var model = new DashboardViewModel
        {
            Username = firstName,
            UpcomingClasses = upcomingCount,
            TotalBookings = bookings.TotalCount,
            ClassesAttended = bookings.Items.Count(b => b.SessionEnd <= now),
            LogHistory = attendances.OrderByDescending(a => a.CheckIn).Take(7).ToList(),
            PaymentHistory = payments.Items.OrderByDescending(p => p.PaymentDate).Take(10).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> AttendanceHistory(int page = 1)
    {
        const int pageSize = 10;
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        var attendances = await _api.GetMyAttendancesAsync(memberId);
        var ordered = attendances.OrderByDescending(a => a.CheckIn).ToList();

        var totalCount = ordered.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var model = new AttendanceHistoryViewModel
        {
            Attendances = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };

        return View(model);
    }
}