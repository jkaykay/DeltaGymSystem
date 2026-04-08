using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

// Management controller for gym equipment.
// Equipment is assigned to a room and can be marked operational or out-of-service.
// Create, Edit, and Delete are Admin-only.
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class EquipmentController : Controller
{
    private readonly IManagementApiService _api;

    public EquipmentController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
        string? search = null, bool? operational = null, int? roomId = null,
        DateTime? dateFrom = null, DateTime? dateTo = null,
        string? sortBy = null, string? sortDir = null)
    {
        var equipment = await _api.GetEquipmentAsync(page, pageSize, search, operational,
            roomId, dateFrom, dateTo, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["Operational"] = operational;
        ViewData["RoomId"] = roomId;
        ViewData["DateFrom"] = dateFrom;
        ViewData["DateTo"] = dateTo;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;
        ViewBag.Rooms = await _api.GetAllRoomsAsync();

        return View(equipment);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _api.GetEquipmentItemAsync(id);
        if (item is null)
            return NotFound();

        return View(item);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Rooms = await _api.GetAllRoomsAsync();
        return View(new CreateEquipmentViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateEquipmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Rooms = await _api.GetAllRoomsAsync();
            return View(model);
        }

        var success = await _api.CreateEquipmentAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create equipment.");
            ViewBag.Rooms = await _api.GetAllRoomsAsync();
            return View(model);
        }

        TempData["Success"] = "Equipment created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _api.GetEquipmentItemAsync(id);
        if (item is null)
            return NotFound();

        ViewBag.Rooms = await _api.GetAllRoomsAsync();

        var vm = new EditEquipmentViewModel
        {
            EquipmentId = item.EquipmentId,
            Description = item.Description,
            InDate = item.InDate,
            Operational = item.Operational,
            RoomId = item.RoomId
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(EditEquipmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Rooms = await _api.GetAllRoomsAsync();
            return View(model);
        }

        var success = await _api.UpdateEquipmentAsync(model.EquipmentId, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update equipment.");
            ViewBag.Rooms = await _api.GetAllRoomsAsync();
            return View(model);
        }

        TempData["Success"] = "Equipment updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteEquipmentAsync(id);

        if (success)
            TempData["Success"] = "Equipment deleted.";
        else
            TempData["Error"] = "Failed to delete equipment.";

        return RedirectToAction(nameof(Index));
    }
}
