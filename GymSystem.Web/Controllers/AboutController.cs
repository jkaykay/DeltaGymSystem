using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
