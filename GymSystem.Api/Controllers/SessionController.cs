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
            var result = await _context.Sessions
                .Select(s => new SessionDTO
                {
                    SessionId = s.SessionId,
                    Start = s.Start,
                    End = s.End,
                    ClassId = s.Class.ClassId,
                    Subject = s.Class.Subject,
                    RoomId = s.Room.RoomId,
                    RoomNumber = s.Room.RoomNumber,
                    MaxCapacity = s.MaxCapacity,
                    BookingCount = s.Bookings.Count,
                    InstructorId = s.Class.User.Id,
                    InstructorName = $"{s.Class.User.FirstName} {s.Class.User.LastName}"
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _context.Sessions
                .Where(s => s.SessionId == id)
                .Select(s => new SessionDTO
                {
                    SessionId = s.SessionId,
                    Start = s.Start,
                    End = s.End,
                    ClassId = s.Class.ClassId,
                    Subject = s.Class.Subject,
                    RoomId = s.Room.RoomId,
                    RoomNumber = s.Room.RoomNumber,
                    MaxCapacity = s.MaxCapacity,
                    BookingCount = s.Bookings.Count,
                    InstructorId = s.Class.User.Id,
                    InstructorName = $"{s.Class.User.FirstName} {s.Class.User.LastName}"
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddSessionRequest request)
        {
            if (request.Start >= request.End)
            {
                return BadRequest("Session start time must be before the end time.");
            }

            var classEntity = await _context.Classes
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ClassId == request.ClassId);

            if (classEntity == null)
            {
                return BadRequest("Class not found.");
            }

            var roomEntity = await _context.Rooms.FindAsync(request.RoomId);
            if (roomEntity == null)
            {
                return BadRequest("Room not found.");
            }

            var hasRoomConflict = await _context.Sessions.AnyAsync(s =>
                s.RoomId == roomEntity.RoomId &&
                s.Start < request.End &&
                s.End > request.Start);

            if (hasRoomConflict)
            {
                return Conflict($"Room {roomEntity.RoomNumber} is already booked during this time slot.");
            }

            var hasClassConflict = await _context.Sessions.AnyAsync(s =>
                s.ClassId == classEntity.ClassId &&
                s.Start < request.End &&
                s.End > request.Start);

            if (hasClassConflict)
            {
                return Conflict($"A session for '{classEntity.Subject}' already exists during this time slot.");
            }

            var session = new Session
            {
                Start = request.Start,
                End = request.End,
                MaxCapacity = request.MaxCapacity,
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
                ClassId = session.Class.ClassId,
                Subject = session.Class.Subject,
                RoomId = session.Room.RoomId,
                RoomNumber = session.Room.RoomNumber,
                MaxCapacity = session.MaxCapacity,
                BookingCount = 0,
                InstructorId = session.Class.User.Id,
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
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
            {
                return NotFound();
            }

            var newStart = request.Start ?? session.Start;
            var newEnd = request.End ?? session.End;
            var newRoomId = request.RoomId ?? session.RoomId;
            var newMaxCapacity = request.MaxCapacity ?? session.MaxCapacity;

            if (newStart >= newEnd)
            {
                return BadRequest("Session start time must be before the end time.");
            }

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

            await _context.SaveChangesAsync();

            var bookingCount = await _context.Bookings.CountAsync(b => b.SessionId == id);

            return Ok(new SessionDTO
            {
                SessionId = session.SessionId,
                Start = session.Start,
                End = session.End,
                ClassId = session.Class.ClassId,
                Subject = session.Class.Subject,
                RoomId = session.Room.RoomId,
                RoomNumber = session.Room.RoomNumber,
                MaxCapacity = session.MaxCapacity,
                BookingCount = bookingCount,
                InstructorId = session.Class.User.Id,
                InstructorName = $"{session.Class.User.FirstName} {session.Class.User.LastName}"
            });
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalSessions()
        {
            var totalSessions = await _context.Sessions.CountAsync();
            return Ok(new CountResponse { Count = totalSessions });
        }
    }
}