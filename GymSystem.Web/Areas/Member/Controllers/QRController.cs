using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers
{
    /// <summary>
    /// Displays a QR code that the member can show at the gym entrance
    /// for check-in. Only active members get a QR code; inactive members
    /// are redirected back to the dashboard with an error message.
    /// </summary>
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class QRController : Controller
    {
        private readonly IMemberApiService _api;

        public QRController(IMemberApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(memberId))
                return Unauthorized();

            // Do not issue a QR code to inactive / unsubscribed members
            var profile = await _api.GetMyProfileAsync();
            if (profile is null)
                return Unauthorized();

            if (!profile.Active)
            {
                TempData["Error"] = "Your membership is inactive. QR codes are only available for active members.";
                return RedirectToAction("Index", "Dashboard", new { area = "Member" });
            }

            var qrcode = await _api.GetMyQRAsync(memberId);
            return View(qrcode);
        }
    }
}