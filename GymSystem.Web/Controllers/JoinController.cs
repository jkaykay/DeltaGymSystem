using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers
{
    public class JoinController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}