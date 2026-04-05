using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;

namespace GymSystem.Web.Controllers;

public class MembershipsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}