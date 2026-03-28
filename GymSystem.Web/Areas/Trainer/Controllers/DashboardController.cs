using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}