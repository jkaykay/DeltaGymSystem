using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class RoomsController : Controller
{
    private readonly IManagementApiService _api;

    public RoomsController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _api.GetRoomsAsync();
        return View(rooms);
    }

    public async Task<IActionResult> Details(int id)
    {
        var room = await _api.GetRoomAsync(id);
        if (room is null)
            return NotFound();

        return View(room);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Branches = await _api.GetBranchesAsync();
        return View(new CreateRoomViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateRoomViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = await _api.GetBranchesAsync();
            return View(model);
        }

        var success = await _api.CreateRoomAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create room. The room number may already exist in this branch.");
            ViewBag.Branches = await _api.GetBranchesAsync();
            return View(model);
        }

        TempData["Success"] = "Room created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _api.GetRoomAsync(id);
        if (room is null)
            return NotFound();

        ViewBag.Branches = await _api.GetBranchesAsync();

        var vm = new EditRoomViewModel
        {
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            BranchId = room.BranchId,
            MaxCapacity = room.MaxCapacity
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(EditRoomViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = await _api.GetBranchesAsync();
            return View(model);
        }

        var success = await _api.UpdateRoomAsync(model.RoomId, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update room. The room number may already exist in this branch.");
            ViewBag.Branches = await _api.GetBranchesAsync();
            return View(model);
        }

        TempData["Success"] = "Room updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteRoomAsync(id);

        if (success)
            TempData["Success"] = "Room deleted.";
        else
            TempData["Error"] = "Failed to delete room.";

        return RedirectToAction(nameof(Index));
    }
}