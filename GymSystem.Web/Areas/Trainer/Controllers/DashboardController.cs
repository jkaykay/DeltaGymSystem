using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymSystem.Web.Areas.Trainer.ViewModels;
using GymSystem.Web.Services;
using GymSystem.Shared.DTOs;


namespace GymSystem.Web.Areas.Trainer.Controllers
{
    [Authorize(Roles = "Trainer")]
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

            var todaySessions = new List<SessionDTO>();

            foreach (var session in sessions)
            {


                if (session.InstructorId == trainerId && session.Start.Date == today)
                {
                    todaySessions.Add(session);
                }
            }

            todaySessions = todaySessions.OrderBy(s => s.Start).ToList();

            var model = new TrainerDashboardViewModel
            {
                TrainerName = trainerName,
                TodaySessions = todaySessions
            };

            return View(model);

        }
    }
}