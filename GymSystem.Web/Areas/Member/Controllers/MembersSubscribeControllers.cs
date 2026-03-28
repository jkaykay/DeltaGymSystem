using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;

namespace GymSystem.Web.Areas.Member.Controllers;

[Area("Member")]
public class SubscribeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}