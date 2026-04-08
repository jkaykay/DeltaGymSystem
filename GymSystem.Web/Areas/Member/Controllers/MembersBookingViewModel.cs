using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace GymSystem.Web.Areas.Member.Controllers;

/// <summary>
/// Lets the logged-in member browse upcoming sessions, book into a session,
/// and cancel existing bookings.
/// Index loads all upcoming sessions plus the member's current bookings
/// so the view can show which sessions are already booked.
/// </summary>
[Authorize(Roles = "Member")]
[Area("Member")]
public class BookingController : Controller
{
    private readonly IMemberApiService _api;

    public BookingController(IMemberApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var sessions = await _api.GetUpcomingSessionsAsync();
        var myBookings = await _api.GetMyBookingsAsync();

        var model = new BookingViewModel
        {
            Sessions = sessions.Items,
            MyBookings = myBookings.Items
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int sessionId)
    {
        var result = await _api.CreateMyBookingAsync(sessionId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error ?? "Booking failed.";
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = "Booking confirmed!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int bookingId)
    {
        var result = await _api.CancelMyBookingAsync(bookingId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error ?? "Cancellation failed.";
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = "Booking cancelled.";
        return RedirectToAction("Index");
    }
}