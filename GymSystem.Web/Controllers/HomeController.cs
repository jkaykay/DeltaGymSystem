using GymSystem.Web.Services;
using GymSystem.Web.Areas.Trainer.ViewModels;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GymSystem.Web.Controllers
{
    /// <summary>
    /// Public-facing controller for the home page, privacy page, and error page.
    /// This is the first page visitors see when they open the website.
    /// It fetches upcoming sessions, membership tiers, and featured trainers
    /// from the API to display on the landing page.
    /// </summary>
    public class HomeController : Controller
    {
        // Service that calls the backend API for member/public data.
        private readonly IMemberApiService _api;

        // Constructor — DI injects the member API service.
        public HomeController(IMemberApiService api)
        {
            _api = api;
        }

        /// <summary>
        /// GET / — The landing page.
        /// Loads upcoming sessions, membership tiers, and random trainers
        /// to showcase on the homepage.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var sessions = await _api.GetUpcomingSessionsAsync(1, 3);
            var tiers = await _api.GetAllTiersAsync();
            var trainers = await _api.GetRandomTrainersAsync(3);

            // ViewBag is a dynamic object that passes data from the controller to the view.
            ViewBag.Sessions = sessions.Items;
            ViewBag.Tiers = tiers;
            ViewBag.Trainers = trainers;

            return View();
        }

        /// <summary>GET /Home/Privacy — displays the privacy policy page.</summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays a generic error page. ResponseCache is disabled so
        /// error pages are never cached by the browser.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>Shown when a user tries to access a page they don’t have permission for.</summary>
        public IActionResult AccessDenied() => View();
    }
}
