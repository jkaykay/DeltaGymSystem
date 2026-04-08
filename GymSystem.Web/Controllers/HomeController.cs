using GymSystem.Web.Services;
using GymSystem.Web.ViewModels;
using GymSystem.Web.Areas.Trainer.ViewModels;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GymSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemberApiService _api;

        public HomeController(IMemberApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var sessions = await _api.GetUpcomingSessionsAsync(1, 3);
            var tiers = await _api.GetAllTiersAsync();
            var trainers = await _api.GetRandomTrainersAsync(3);

            var model = new HomeViewModel
            {
                Sessions = sessions.Items,
                Tiers = tiers,
                Trainers = trainers
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied() => View();
    }
}
