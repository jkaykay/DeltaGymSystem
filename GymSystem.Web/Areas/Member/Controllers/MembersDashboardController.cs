using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace GymSystem.Web.Areas.Member.Controllers;

[Authorize]
[Area("Member")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}