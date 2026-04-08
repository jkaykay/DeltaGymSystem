using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

/// <summary>
/// Management controller for trainers.
/// Provides list (with paging/filtering), details, create, edit, and delete.
/// Uses the same +44 phone-number helpers as the Members controller.
/// </summary>
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class TrainersController : Controller
{
    private readonly IManagementApiService _api;

    public TrainersController(IManagementApiService api)
    {
        _api = api;
    }

    private static string? StripUkPrefix(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Replace(" ", "");
        if (phone.StartsWith("+44")) phone = phone[3..];
        return phone;
    }

    private static string? PrependUkPrefix(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Replace(" ", "").TrimStart('0');
        return "+44" + phone;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10,
    string? search = null,
    DateTime? hiredFrom = null, DateTime? hiredTo = null,
    string? sortBy = null, string? sortDir = null)
    {
        var result = await _api.GetTrainersAsync(page, pageSize, search,
            hiredFrom, hiredTo, sortBy, sortDir);

        ViewData["Search"] = search;
        ViewData["HiredFrom"] = hiredFrom;
        ViewData["HiredTo"] = hiredTo;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        return View(result);
    }

    public async Task<IActionResult> Details(string id)
    {
        var trainer = await _api.GetTrainerAsync(id);
        if (trainer is null)
            return NotFound();

        return View(trainer);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Branches = await _api.GetAllBranchesAsync();
        return View(new CreateTrainerViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateTrainerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        model.PhoneNumber = PrependUkPrefix(model.PhoneNumber);

        var success = await _api.CreateTrainerAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create trainer. Check password requirements or duplicate email/employee ID.");
            ViewBag.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        TempData["Success"] = "Trainer created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        var trainer = await _api.GetTrainerAsync(id);
        if (trainer is null)
            return NotFound();

        ViewBag.Branches = await _api.GetAllBranchesAsync();

        var vm = new EditTrainerViewModel
        {
            Id = trainer.Id,
            Email = trainer.Email,
            FirstName = trainer.FirstName,
            LastName = trainer.LastName,
            EmployeeId = trainer.EmployeeId,
            BranchId = trainer.BranchId,
            PhoneNumber = StripUkPrefix(trainer.PhoneNumber)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(EditTrainerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        model.PhoneNumber = PrependUkPrefix(model.PhoneNumber);

        var success = await _api.UpdateTrainerAsync(model.Id, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update trainer.");
            ViewBag.Branches = await _api.GetAllBranchesAsync();
            return View(model);
        }

        TempData["Success"] = "Trainer updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _api.DeleteTrainerAsync(id);

        if (success)
            TempData["Success"] = "Trainer deleted.";
        else
            TempData["Error"] = "Failed to delete trainer.";

        return RedirectToAction(nameof(Index));
    }
}