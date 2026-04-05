using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace GymSystem.Web.Areas.Member.Controllers;

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