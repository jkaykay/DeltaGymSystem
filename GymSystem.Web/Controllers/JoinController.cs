using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers
{
    // Public-facing controller for the Join page.
    // Displays a landing page that directs visitors to sign up or log in.
    public class JoinController : Controller
    {
        // GET /Join — Shows the join/sign-up landing page.
        public IActionResult Index()
        {
            return View();
        }
    }
}
