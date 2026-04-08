using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers;

// Management controller for the QR code scanner page.
// The Index page opens the camera / QR scanner UI.
// When a QR code is scanned, the JavaScript on the page POSTs the token
// to the Scan action, which validates it against the API and returns
// the check-in/check-out result as JSON.
[Area("Management")]
[Authorize(Roles = "Admin,Staff")]
public class QRScannerController : Controller
{
    private readonly IManagementApiService _api;

    public QRScannerController(IManagementApiService api)
    {
        _api = api;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Scan([FromBody] ScanTokenViewModel input)
    {
        if (string.IsNullOrWhiteSpace(input?.Token))
            return Json(new ScanResultViewModel { Success = false, ErrorMessage = "No token received." });

        var result = await _api.ScanQRCodeAsync(input.Token);
        return Json(result);
    }
}
