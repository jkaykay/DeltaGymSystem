using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

/// <summary>
/// Management controller for session bookings.
/// A booking reserves a spot for a member in a session.
/// Staff can view, create, and cancel bookings on behalf of members.
/// </summary>
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class BookingsController : Controller
{
    private readonly IManagementApiService _api;

    public BookingsController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
        string? search = null,
        DateTime? dateFrom = null, DateTime? dateTo = null,
        string? sortBy = null, string? sortDir = null)
    {
        var result = await _api.GetBookingsAsync(page, pageSize, search,
            dateFrom, dateTo, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["DateFrom"] = dateFrom;
        ViewData["DateTo"] = dateTo;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var booking = await _api.GetBookingAsync(id);
        if (booking is null)
            return NotFound();

        return View(booking);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Members = await _api.GetAllMembersAsync();
        ViewBag.Sessions = await _api.GetAllSessionsAsync();
        return View(new CreateBookingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Members = await _api.GetAllMembersAsync();
            ViewBag.Sessions = await _api.GetAllSessionsAsync();
            return View(model);
        }

        var success = await _api.CreateBookingAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create booking. The session may be full or the member may already be booked.");
            ViewBag.Members = await _api.GetAllMembersAsync();
            ViewBag.Sessions = await _api.GetAllSessionsAsync();
            return View(model);
        }

        TempData["Success"] = "Booking created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteBookingAsync(id);

        if (success)
            TempData["Success"] = "Booking cancelled.";
        else
            TempData["Error"] = "Failed to cancel booking.";

        return RedirectToAction(nameof(Index));
    }
}