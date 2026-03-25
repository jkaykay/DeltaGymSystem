using Microsoft.AspNetCore.Mvc;
using GymSystem.Web.Areas.Member.ViewModels;

namespace GymSystem.Web.Areas.Member.Controllers;

[Area("Member")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        // Hardcoded for now — later you'll pull this from your API/database
        var model = new DashboardViewModel
        {
            Username = "John",
            UpcomingClasses = 3,
            TotalBookings = 12,
            ClassesAttended = 9,
            LogHistory = new List<LogItem>
            {
                new LogItem { Name = "Friday afternoon workout", TimeFrom = "14.19", TimeTo = "16.16" },
                new LogItem { Name = "Sunday morning workout", TimeFrom = "08.19", TimeTo = "10.50" },
                new LogItem { Name = "Monday evening workout", TimeFrom = "22.23", TimeTo = "23.52" }
            },
            PaymentHistory = new List<PaymentItem>
            {
                new PaymentItem { Month = "March", Amount = "$49.99" },
                new PaymentItem { Month = "April", Amount = "$49.99" }
            }
        };

        return View(model);
    }
}