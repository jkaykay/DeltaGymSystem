using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Web.Controllers;
public class ClassesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}