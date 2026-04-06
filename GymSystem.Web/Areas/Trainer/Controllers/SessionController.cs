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
    public class SessionController : Controller
    {
        private readonly ITrainerApiService _trainerApiService;

        public SessionController(ITrainerApiService trainerApiService)
        {

            _trainerApiService = trainerApiService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                return Challenge();

            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var trainerName = User.FindFirst("firstName")?.Value ?? "Trainer";

            var sessions = await _trainerApiService.GetSessionsAsync(token);

            var trainerSessions = new List<SessionDTO>();

            foreach (var session in sessions)
            {
                if (session.InstructorId == trainerId)
                {
                    trainerSessions.Add(session);
                }
            }

            trainerSessions = trainerSessions.OrderBy(s => s.Start).ToList();

            var totalCount = trainerSessions.Count;
            var paged = trainerSessions.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new TrainerSessionViewModel
            {
                TrainerName = trainerName,
                WeeklySessions = paged,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(model);

        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                return Challenge();

            var session = await _trainerApiService.GetSessionByIdAsync(id, token);

            if (session == null)
                return NotFound();

            var bookings = await _trainerApiService.GetSessionBookingsAsync(id, token);

            var model = new SessionDetailViewModel
            {
                Session = session,
                Bookings = bookings
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                return Challenge();

            await _trainerApiService.DeleteSessionAsync(id, token);

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                return Challenge();

            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var profile = await _trainerApiService.GetTrainerProfileAsync(token);

            var classes = await _trainerApiService.GetTrainerClassesAsync(trainerId, token);

            var rooms = new List<RoomDTO>();
            if (profile?.BranchId.HasValue == true)
            {
                rooms = await _trainerApiService.GetRoomsByBranchAsync(profile.BranchId.Value, token);
            }

            var model = new CreateSessionViewModel
            {
                Classes = classes,
                Rooms = rooms
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSessionViewModel model)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrWhiteSpace(token))
                return Challenge();

            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            if (!ModelState.IsValid)
            {
                var profile = await _trainerApiService.GetTrainerProfileAsync(token);
                model.Classes = await _trainerApiService.GetTrainerClassesAsync(trainerId, token);
                model.Rooms = profile?.BranchId.HasValue == true
                    ? await _trainerApiService.GetRoomsByBranchAsync(profile.BranchId.Value, token)
                    : new List<RoomDTO>();
                return View(model);
            }

            var request = new AddSessionRequest(
                model.Start,
                model.End,
                model.RoomId,
                model.ClassId,
                model.MaxCapacity
            );

            var success = await _trainerApiService.CreateSessionAsync(request, token);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Failed to create session. There may be a scheduling conflict for the room or class.");
                var profile = await _trainerApiService.GetTrainerProfileAsync(token);
                model.Classes = await _trainerApiService.GetTrainerClassesAsync(trainerId, token);
                model.Rooms = profile?.BranchId.HasValue == true
                    ? await _trainerApiService.GetRoomsByBranchAsync(profile.BranchId.Value, token)
                    : new List<RoomDTO>();
                return View(model);
            }

            TempData["Success"] = "Session created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}