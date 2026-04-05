using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class ScheduleController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly IOutputCacheStore _outputCache;

        public ScheduleController(GymDbContext context, IOutputCacheStore outputCache)
        {
            _context = context;
            _outputCache = outputCache;
        }

        [HttpGet]
        [OutputCache(PolicyName = "schedules")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _context.Schedules
                .Select(s => new ScheduleDTO
                {
                    ScheduleId = s.ScheduleId,
                    Start = s.Start,
                    End = s.End,
                    UserId = s.UserId,
                    UserName = $"{s.User.FirstName} {s.User.LastName}"
                })
                .ToPagedResultAsync(page, pageSize);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [OutputCache(PolicyName = "schedules")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _context.Schedules
                .Where(s => s.ScheduleId == id)
                .Select(s => new ScheduleDTO
                {
                    ScheduleId = s.ScheduleId,
                    Start = s.Start,
                    End = s.End,
                    UserId = s.UserId,
                    UserName = $"{s.User.FirstName} {s.User.LastName}"
                })
                .FirstOrDefaultAsync();

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AddScheduleRequest request)
        {
            if (request.Start >= request.End)
                return BadRequest("Schedule start time must be before the end time.");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return BadRequest("User not found.");

            var hasConflict = await _context.Schedules.AnyAsync(s =>
                s.UserId == request.UserId &&
                s.Start < request.End &&
                s.End > request.Start);

            if (hasConflict)
                return Conflict("User already has a schedule during this time slot.");

            var schedule = new Schedule
            {
                Start = request.Start,
                End = request.End,
                UserId = request.UserId,
                User = user
            };

            _context.Schedules.Add(schedule);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to create schedule.");

            await _outputCache.EvictByTagAsync("schedules", default);

            return CreatedAtAction(nameof(Get), new { id = schedule.ScheduleId }, new ScheduleDTO
            {
                ScheduleId = schedule.ScheduleId,
                Start = schedule.Start,
                End = schedule.End,
                UserId = schedule.UserId,
                UserName = $"{user.FirstName} {user.LastName}"
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleRequest request)
        {
            var schedule = await _context.Schedules
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.ScheduleId == id);

            if (schedule == null) return NotFound();

            var newStart = request.Start ?? schedule.Start;
            var newEnd = request.End ?? schedule.End;
            var newUserId = request.UserId ?? schedule.UserId;

            if (newStart >= newEnd)
                return BadRequest("Schedule start time must be before the end time.");

            if (newUserId != schedule.UserId)
            {
                var user = await _context.Users.FindAsync(newUserId);
                if (user == null)
                    return BadRequest("User not found.");

                schedule.UserId = newUserId;
                schedule.User = user;
            }

            if (newUserId != schedule.UserId || newStart != schedule.Start || newEnd != schedule.End)
            {
                var hasConflict = await _context.Schedules.AnyAsync(s =>
                    s.ScheduleId != id &&
                    s.UserId == newUserId &&
                    s.Start < newEnd &&
                    s.End > newStart);

                if (hasConflict)
                    return Conflict("User already has a schedule during this time slot.");
            }

            schedule.Start = newStart;
            schedule.End = newEnd;

            await _context.SaveChangesAsync();

            await _outputCache.EvictByTagAsync("schedules", default);

            return Ok(new ScheduleDTO
            {
                ScheduleId = schedule.ScheduleId,
                Start = schedule.Start,
                End = schedule.End,
                UserId = schedule.UserId,
                UserName = $"{schedule.User.FirstName} {schedule.User.LastName}"
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null) return NotFound();

            _context.Schedules.Remove(schedule);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to delete schedule.");

            await _outputCache.EvictByTagAsync("schedules", default);

            return NoContent();
        }

        [HttpGet("total")]
        [OutputCache(PolicyName = "schedules")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _context.Schedules.CountAsync();
            return Ok(new CountResponse { Count = total });
        }
    }
}
