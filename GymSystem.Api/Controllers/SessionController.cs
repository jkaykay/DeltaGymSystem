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
    [Authorize(Roles = "Admin,Staff,Trainer")]
    public class SessionController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public SessionController(GymDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await _context.Sessions
                .Include(s => s.Class)
                    .ThenInclude(c => c.User)
                .Include(s => s.Room)
                .Include(s => s.Bookings)
                .ToListAsync();

            var result = sessions.Select(s => new SessionDTO
            {
                SessionId = s.SessionId,
                Start = s.Start,
                End = s.End,
                Subject = s.Class.Subject,
                RoomNumber = s.Room.RoomNumber,
                MaxCapacity = s.MaxCapacity,
                BookingCount = s.Bookings.Count,
                InstructorName = $"{s.Class.User.FirstName} {s.Class.User.LastName}"
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var session = await _context.Sessions
                .Include(s => s.Class)
                    .ThenInclude(c => c.User)
                .Include(s => s.Room)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
            {
                return NotFound();
            }

            var result = new SessionDTO
            {
                SessionId = session.SessionId,
                Start = session.Start,
                End = session.End,
                Subject = session.Class.Subject,
                RoomNumber = session.Room.RoomNumber,
                MaxCapacity = session.MaxCapacity,
                BookingCount = session.Bookings.Count,
                InstructorName = $"{session.Class.User.FirstName} {session.Class.User.LastName}"
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SessionDTO sessionDto)
        {
            var classEntity = await _context.Classes
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Subject == sessionDto.Subject);
            var roomEntity = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == sessionDto.RoomNumber);
            if (classEntity == null || roomEntity == null)
            {
                return BadRequest("Invalid class or room information.");
            }

            var hasRoomConflict = await _context.Sessions.AnyAsync(s =>
                s.RoomId == roomEntity.RoomId &&
                s.Start < sessionDto.End &&
                s.End > sessionDto.Start);

            if (hasRoomConflict)
            {
                return Conflict($"Room {sessionDto.RoomNumber} is already booked during this time slot.");
            }

            var hasClassConflict = await _context.Sessions.AnyAsync(s =>
                s.ClassId == classEntity.ClassId &&
                s.Start < sessionDto.End &&
                s.End > sessionDto.Start);

            if (hasClassConflict)
            {
                return Conflict($"A session for '{sessionDto.Subject}' already exists during this time slot.");
            }

            var session = new Session
            {
                Start = sessionDto.Start,
                End = sessionDto.End,
                MaxCapacity = sessionDto.MaxCapacity,
                ClassId = classEntity.ClassId,
                Class = classEntity,
                RoomId = roomEntity.RoomId,
                Room = roomEntity
            };
            _context.Sessions.Add(session);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to create session.");

            return CreatedAtAction(nameof(Get), new { id = session.SessionId }, new SessionDTO
            {
                SessionId = session.SessionId,
                Start = session.Start,
                End = session.End,
                Subject = session.Class.Subject,
                RoomNumber = session.Room.RoomNumber,
                MaxCapacity = session.MaxCapacity,
                BookingCount = session.Bookings.Count,
                InstructorName = $"{session.Class.User.FirstName} {session.Class.User.LastName}"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            _context.Sessions.Remove(session);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to delete session.");

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSessionRequest request)
        {
            var session = await _context.Sessions
                .Include(s => s.Class)
                    .ThenInclude(c => c.User)
                .Include(s => s.Room)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
            {
                return NotFound();
            }

            var newStart = request.Start ?? session.Start;
            var newEnd = request.End ?? session.End;
            var newRoomId = request.RoomId ?? session.RoomId;
            var newMaxCapacity = request.MaxCapacity ?? session.MaxCapacity;

            // Only check room conflict if the room or time window changed
            if (newRoomId != session.RoomId || newStart != session.Start || newEnd != session.End)
            {
                var hasRoomConflict = await _context.Sessions.AnyAsync(s =>
                    s.SessionId != id &&
                    s.RoomId == newRoomId &&
                    s.Start < newEnd &&
                    s.End > newStart);

                if (hasRoomConflict)
                {
                    var roomNumber = newRoomId != session.RoomId
                        ? (await _context.Rooms.FindAsync(newRoomId))?.RoomNumber ?? newRoomId
                        : session.Room.RoomNumber;
                    return Conflict($"Room {roomNumber} is already booked during this time slot.");
                }
            }

            // Only check class conflict if the time window changed
            if (newStart != session.Start || newEnd != session.End)
            {
                var hasClassConflict = await _context.Sessions.AnyAsync(s =>
                    s.SessionId != id &&
                    s.ClassId == session.ClassId &&
                    s.Start < newEnd &&
                    s.End > newStart);

                if (hasClassConflict)
                {
                    return Conflict($"A session for '{session.Class.Subject}' already exists during this time slot.");
                }
            }

            // Look up the new room if it changed
            if (newRoomId != session.RoomId)
            {
                var roomEntity = await _context.Rooms.FindAsync(newRoomId);
                if (roomEntity == null)
                {
                    return BadRequest("Invalid room information.");
                }
                session.RoomId = roomEntity.RoomId;
                session.Room = roomEntity;
            }

            session.Start = newStart;
            session.End = newEnd;
            session.MaxCapacity = newMaxCapacity;

            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to update session.");

            return Ok(new SessionDTO
            {
                SessionId = session.SessionId,
                Start = session.Start,
                End = session.End,
                Subject = session.Class.Subject,
                RoomNumber = session.Room.RoomNumber,
                MaxCapacity = session.MaxCapacity,
                BookingCount = session.Bookings.Count,
                InstructorName = $"{session.Class.User.FirstName} {session.Class.User.LastName}"
            });
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalSessions()
        {
            var totalSessions = await _context.Sessions.ToListAsync();
            return Ok(new CountResponse { Count = totalSessions.Count });
        }
    }
}