using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymSystem.Web.Areas.Trainer.ViewModels;
using GymSystem.Web.Services;


namespace GymSystem.Web.Areas.Trainer.Controllers
{
    [Authorize(Roles = "Trainer,Admin")]
    [Area("Trainer")]
    public class DashboardController : Controller
    {
        private readonly ITrainerApiService _trainerApiService;

        public DashboardController(ITrainerApiService trainerApiService)
        {

            _trainerApiService = trainerApiService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                return Challenge();

            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var trainerName = User.FindFirst("firstName")?.Value ?? "Trainer";

            var today = DateTime.Today;

            // Fetch today's sessions for this trainer
            var todayResult = await _trainerApiService.GetSessionsByTrainerAsync(
                trainerId, today, today.AddDays(1).AddTicks(-1), 1, 50, token);

            // Fetch upcoming sessions (after today) for this trainer
            var upcomingResult = await _trainerApiService.GetSessionsByTrainerAsync(
                trainerId, today.AddDays(1), null, 1, 10, token);

            var model = new TrainerDashboardViewModel
            {
                TrainerName = trainerName,
                TodaySessions = todayResult.Items.OrderBy(s => s.Start).ToList(),
                UpcomingSessions = upcomingResult.Items.OrderBy(s => s.Start).ToList()
            };

            return View(model);

        }
    }
}