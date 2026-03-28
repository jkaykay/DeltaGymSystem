using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Management.Controllers
{
    public class TrainerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
