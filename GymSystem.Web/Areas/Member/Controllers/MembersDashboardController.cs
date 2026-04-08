using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers;

// The member’s personal dashboard.
// Shows a summary (upcoming bookings, recent attendance, payment history)
// and provides sub-pages for full attendance history, booking history, and payment history.
// Requires the Member role.
[Authorize(Roles = "Member")]
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
        var upcomingBookings = bookings.Items
            .Where(b => b.SessionStart > now)
            .OrderBy(b => b.SessionStart)
            .Take(3)
            .ToList();

        var model = new DashboardViewModel
        {
            Username = firstName,
            TotalBookings = bookings.Items.Count(b => b.SessionStart > now), // upcoming only
            TotalAttendances = attendances.Count,
            UpcomingBookings = upcomingBookings,
            LogHistory = attendances.OrderByDescending(a => a.CheckIn).Take(7).ToList(),
            PaymentHistory = payments.Items.OrderByDescending(p => p.PaymentDate).Take(5).ToList()
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

    public async Task<IActionResult> BookingHistory(int page = 1, string? search = null)
    {
        const int pageSize = 10;

        var bookings = await _api.GetMyBookingsAsync(search: search);
        var ordered = bookings.Items.OrderByDescending(b => b.SessionStart).ToList();

        var totalCount = ordered.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var model = new BookingHistoryViewModel
        {
            Search = search,
            Bookings = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };

        return View(model);
    }

    public async Task<IActionResult> PaymentHistory(int page = 1)
    {
        const int pageSize = 10;

        var payments = await _api.GetMyPaymentsAsync();
        var ordered = payments.Items.OrderByDescending(p => p.PaymentDate).ToList();

        var totalCount = ordered.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var model = new PaymentHistoryViewModel
        {
            Payments = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };

        return View(model);
    }
}
