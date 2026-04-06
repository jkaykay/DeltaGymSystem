using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymSystem.Web.Areas.Trainer.ViewModels;
using GymSystem.Web.Services;
using GymSystem.Shared.DTOs;


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

            var sessions = await _trainerApiService.GetSessionsAsync(token);

            var today = DateTime.Today;
            var now = DateTime.Now;

            var todaySessions = new List<SessionDTO>();
            var upcomingSessions = new List<SessionDTO>();

            foreach (var session in sessions)
            {
                if (session.InstructorId == trainerId)
                {
                    if (session.Start.Date == today)
                    {
                        todaySessions.Add(session);
                    }

                    if (session.Start > now && session.Start.Date > today)
                    {
                        upcomingSessions.Add(session);
                    }
                }
            }

            todaySessions = todaySessions.OrderBy(s => s.Start).ToList();
            upcomingSessions = upcomingSessions.OrderBy(s => s.Start).Take(10).ToList();

            var model = new TrainerDashboardViewModel
            {
                TrainerName = trainerName,
                TodaySessions = todaySessions,
                UpcomingSessions = upcomingSessions
            };

            return View(model);

        }
    }
}