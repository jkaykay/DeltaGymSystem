using GymSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymSystem.Web.Areas.Member.Controllers
{
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

            var qrcode = await _api.GetMyQRAsync(memberId);
            return View(qrcode);
        }
    }
}