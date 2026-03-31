using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class SessionsController : Controller
{
    private readonly IManagementApiService _api;

    public SessionsController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var result = await _api.GetSessionsAsync(page, pageSize);
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var session = await _api.GetSessionAsync(id);
        if (session is null)
            return NotFound();

        return View(session);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Classes = await _api.GetAllClassesAsync();
        ViewBag.Rooms = await _api.GetAllRoomsAsync();
        return View(new CreateSessionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateSessionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Classes = await _api.GetAllClassesAsync();
            ViewBag.Rooms = await _api.GetAllRoomsAsync();
            return View(model);
        }

        var success = await _api.CreateSessionAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create session. There may be a scheduling conflict for the room or class.");
            ViewBag.Classes = await _api.GetAllClassesAsync();
            ViewBag.Rooms = await _api.GetAllRoomsAsync();
            return View(model);
        }

        TempData["Success"] = "Session created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var session = await _api.GetSessionAsync(id);
        if (session is null)
            return NotFound();

        ViewBag.Rooms = await _api.GetAllRoomsAsync();

        var vm = new EditSessionViewModel
        {
            SessionId = session.SessionId,
            ClassId = session.ClassId,
            Subject = session.Subject,
            RoomId = session.RoomId,
            Start = session.Start,
            End = session.End,
            MaxCapacity = session.MaxCapacity
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(EditSessionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Rooms = await _api.GetAllRoomsAsync();
            return View(model);
        }

        var success = await _api.UpdateSessionAsync(model.SessionId, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update session. There may be a scheduling conflict for the room or class.");
            ViewBag.Rooms = await _api.GetAllRoomsAsync();
            return View(model);
        }

        TempData["Success"] = "Session updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteSessionAsync(id);

        if (success)
            TempData["Success"] = "Session deleted.";
        else
            TempData["Error"] = "Failed to delete session.";

        return RedirectToAction(nameof(Index));
    }
}