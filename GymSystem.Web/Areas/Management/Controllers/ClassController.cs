using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

/// <summary>
/// Management controller for gym classes (e.g. Yoga, Boxing, Spin).
/// Each class is assigned to a trainer. Create, Edit, and Delete are Admin-only.
/// </summary>
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class ClassesController : Controller
{
    private readonly IManagementApiService _api;

    public ClassesController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
        string? search = null,
        string? sortBy = null, string? sortDir = null)
    {
        var result = await _api.GetClassesAsync(page, pageSize, search, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var gymClass = await _api.GetClassAsync(id);
        if (gymClass is null)
            return NotFound();

        return View(gymClass);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Trainers = await _api.GetAllTrainersAsync();
        return View(new CreateClassViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateClassViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Trainers = await _api.GetAllTrainersAsync();
            return View(model);
        }

        var success = await _api.CreateClassAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create class. This subject may already exist for the selected trainer.");
            ViewBag.Trainers = await _api.GetAllTrainersAsync();
            return View(model);
        }

        TempData["Success"] = "Class created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var gymClass = await _api.GetClassAsync(id);
        if (gymClass is null)
            return NotFound();

        ViewBag.Trainers = await _api.GetAllTrainersAsync();

        var vm = new EditClassViewModel
        {
            ClassId = gymClass.ClassId,
            Subject = gymClass.Subject,
            UserId = gymClass.UserId
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(EditClassViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Trainers = await _api.GetAllTrainersAsync();
            return View(model);
        }

        var success = await _api.UpdateClassAsync(model.ClassId, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update class. This subject may already exist for the selected trainer.");
            ViewBag.Trainers = await _api.GetAllTrainersAsync();
            return View(model);
        }

        TempData["Success"] = "Class updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _api.DeleteClassAsync(id);

        if (success)
            TempData["Success"] = "Class deleted.";
        else
            TempData["Error"] = "Failed to delete class.";

        return RedirectToAction(nameof(Index));
    }
}