using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class ScheduleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}