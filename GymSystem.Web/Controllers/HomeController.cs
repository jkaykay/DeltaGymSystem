using GymSystem.Web.Services;
using GymSystem.Web.Areas.Trainer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GymSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IManagementApiService _api;

        public HomeController(IManagementApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            return View();
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
    }
}
