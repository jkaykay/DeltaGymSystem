using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers
{
    /// <summary>
    /// Public-facing controller for the Join page.
    /// Displays a landing page that directs visitors to sign up or log in.
    /// </summary>
    public class JoinController : Controller
    {
        /// <summary>GET /Join — Shows the join/sign-up landing page.</summary>
        public IActionResult Index()
        {
            return View();
        }
    }
}