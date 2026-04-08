using GymSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers
{
    // Public-facing controller for the About page.
    // Displays general information about the gym and lists the available classes.
    public class AboutController : Controller
    {
        private readonly IMemberApiService _api;

        public AboutController(IMemberApiService api)
        {
            _api = api;
        }

        // GET /About — Fetches all gym classes and passes them to the view.
        public async Task<IActionResult> Index()
        {
            var classes = await _api.GetClassesAsync();
            ViewBag.Classes = classes.Items;

            return View();
        }
    }
}
