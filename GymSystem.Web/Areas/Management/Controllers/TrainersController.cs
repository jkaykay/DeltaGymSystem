using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class TrainersController : Controller
{
    private readonly IManagementApiService _api;

    public TrainersController(IManagementApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var result = await _api.GetTrainersAsync(page, pageSize);
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
        ViewBag.Branches = await _api.GetBranchesAsync();
        return View(new CreateTrainerViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateTrainerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = await _api.GetBranchesAsync();
            return View(model);
        }

        var success = await _api.CreateTrainerAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create trainer. Check password requirements or duplicate email/employee ID.");
            ViewBag.Branches = await _api.GetBranchesAsync();
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

        ViewBag.Branches = await _api.GetBranchesAsync();

        var vm = new EditTrainerViewModel
        {
            Id = trainer.Id,
            Email = trainer.Email,
            FirstName = trainer.FirstName,
            LastName = trainer.LastName,
            EmployeeId = trainer.EmployeeId,
            BranchId = trainer.BranchId
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
            ViewBag.Branches = await _api.GetBranchesAsync();
            return View(model);
        }

        var success = await _api.UpdateTrainerAsync(model.Id, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update trainer.");
            ViewBag.Branches = await _api.GetBranchesAsync();
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