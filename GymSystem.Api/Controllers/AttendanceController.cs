using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(GymDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var attendances = await _context.Attendances.Include(a => a.User).ToListAsync();
            var result = attendances.Select(a => new AttendanceDTO
            {
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                UserId = a.UserId,
                MemberName = $"{a.User.FirstName} {a.User.LastName}",
                InFlag = a.InFlag
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var attendance = await _context.Attendances.Include(a => a.User).FirstOrDefaultAsync(a => a.AttendanceId == id);

            if (attendance == null) return NotFound("Specified attendance record does not exist.");

            return Ok(new AttendanceDTO
            {
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                UserId = attendance.UserId,
                MemberName = $"{attendance.User.FirstName} {attendance.User.LastName}",
                InFlag = attendance.InFlag
            });
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetMyAttendances(string memberId)
        {
            var user = await _userManager.FindByIdAsync(memberId);
            if (user is null) return NotFound($"No user with ID '{memberId}' was found.");

            var memberAttendances = await _context.Attendances
                .Include(a => a.User)
                .Where(a => a.UserId == memberId)
                .ToListAsync();

            if (!memberAttendances.Any())
                return NotFound($"No attendance records for that user exist yet.");

            var result = memberAttendances.Select(a => new AttendanceDTO
            {
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                UserId = a.UserId,
                MemberName = $"{a.User.FirstName} {a.User.LastName}",
                InFlag = a.InFlag
            });

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Create([FromBody] AddAttendanceRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null) return NotFound($"No user with ID '{request.UserId}' was found.");

            var attendance = new Attendance
            {
                CheckIn = request.CheckIn,
                CheckOut = default,
                InFlag = true,
                UserId = request.UserId,
                User = user
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = attendance.AttendanceId }, new AttendanceDTO
            {
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                UserId = attendance.UserId,
                MemberName = $"{user.FirstName} {user.LastName}",
                InFlag = attendance.InFlag
            });
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] AddAttendanceRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null) return NotFound($"No user with ID '{request.UserId}' was found.");

            var openSession = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.InFlag);

            if (openSession is not null)
                return Conflict("This member already has an active check-in session.");

            var attendance = new Attendance
            {
                CheckIn = request.CheckIn,
                CheckOut = default,
                InFlag = true,
                UserId = request.UserId,
                User = user
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = attendance.AttendanceId }, new AttendanceDTO
            {
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                UserId = attendance.UserId,
                MemberName = $"{user.FirstName} {user.LastName}",
                InFlag = attendance.InFlag
            });
        }

        [HttpPut("checkout/{memberId}")]
        public async Task<IActionResult> CheckOut(string memberId, [FromBody] UpdateAttendanceRequest request)
        {
            var openSession = await _context.Attendances
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == memberId && a.InFlag);

            if (openSession is null)
                return NotFound("No active check-in session found for this member.");

            openSession.CheckOut = request.CheckOut ?? DateTime.UtcNow;
            openSession.InFlag = false;

            if (request.CheckIn.HasValue) openSession.CheckIn = request.CheckIn.Value;

            await _context.SaveChangesAsync();

            return Ok(new AttendanceDTO
            {
                CheckIn = openSession.CheckIn,
                CheckOut = openSession.CheckOut,
                UserId = openSession.UserId,
                MemberName = $"{openSession.User.FirstName} {openSession.User.LastName}",
                InFlag = openSession.InFlag
            });
        }
    }
}