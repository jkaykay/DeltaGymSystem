using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Areas.Member.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
