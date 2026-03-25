using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOutputCacheStore _outputCache;

        public BookingController(GymDbContext context, UserManager<ApplicationUser> userManager, IOutputCacheStore outputCache)
        {
            _context = context;
            _userManager = userManager;
            _outputCache = outputCache;
        }

        // --- Admin/Staff endpoints ---

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [OutputCache(PolicyName = "bookings")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.Bookings
                .Select(b => new BookingDTO
                {
                    BookingId = b.BookingId,
                    BookDate = b.BookDate,
                    SessionId = b.Session.SessionId,
                    SessionStart = b.Session.Start,
                    SessionEnd = b.Session.End,
                    Subject = b.Session.Class.Subject,
                    RoomNumber = b.Session.Room.RoomNumber,
                    UserId = b.User.Id,
                    UserName = $"{b.User.FirstName} {b.User.LastName}"
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [OutputCache(PolicyName = "bookings")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _context.Bookings
                .Where(b => b.BookingId == id)
                .Select(b => new BookingDTO
                {
                    BookingId = b.BookingId,
                    BookDate = b.BookDate,
                    SessionId = b.Session.SessionId,
                    SessionStart = b.Session.Start,
                    SessionEnd = b.Session.End,
                    Subject = b.Session.Class.Subject,
                    RoomNumber = b.Session.Room.RoomNumber,
                    UserId = b.User.Id,
                    UserName = $"{b.User.FirstName} {b.User.LastName}"
                })
                .FirstOrDefaultAsync();

            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Staff")]  // Restricted — members use GET my instead
        [OutputCache(PolicyName = "bookings")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound("User not found.");

            var result = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Select(b => new BookingDTO
                {
                    BookingId = b.BookingId,
                    BookDate = b.BookDate,
                    SessionId = b.Session.SessionId,
                    SessionStart = b.Session.Start,
                    SessionEnd = b.Session.End,
                    Subject = b.Session.Class.Subject,
                    RoomNumber = b.Session.Room.RoomNumber,
                    UserId = b.User.Id,
                    UserName = $"{b.User.FirstName} {b.User.LastName}"
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]  // Admin/Staff book on behalf of a user
        public async Task<IActionResult> Create([FromBody] AddBookingRequest request)
        {
            return await ProcessBookingAsync(request.UserId, request.SessionId);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]  // Admin/Staff can delete any booking
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking is null)
                return NotFound();

            _context.Bookings.Remove(booking);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to delete booking.");

            await _outputCache.EvictByTagAsync("bookings", default);
            return NoContent();
        }

        [HttpGet("total")]
        [Authorize(Roles = "Admin,Staff")]
        [OutputCache(PolicyName = "bookings")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _context.Bookings.CountAsync();
            return Ok(new CountResponse { Count = total });
        }

        // --- Member self-service endpoints ---

        [HttpGet("my")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMy()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Select(b => new BookingDTO
                {
                    BookingId = b.BookingId,
                    BookDate = b.BookDate,
                    SessionId = b.Session.SessionId,
                    SessionStart = b.Session.Start,
                    SessionEnd = b.Session.End,
                    Subject = b.Session.Class.Subject,
                    RoomNumber = b.Session.Room.RoomNumber,
                    UserId = b.User.Id,
                    UserName = $"{b.User.FirstName} {b.User.LastName}"
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost("my")]
        [Authorize(Roles = "Member")]  // UserId comes from claims — members can only book for themselves
        public async Task<IActionResult> CreateMy([FromBody] AddMyBookingRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return await ProcessBookingAsync(userId, request.SessionId);
        }

        [HttpDelete("my/{id}")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> DeleteMy(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var booking = await _context.Bookings.FindAsync(id);
            if (booking is null)
                return NotFound();

            // Ownership check — members can only cancel their own bookings
            if (booking.UserId != userId)
                return Forbid();

            _context.Bookings.Remove(booking);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to delete booking.");

            await _outputCache.EvictByTagAsync("bookings", default);
            return NoContent();
        }

        // --- Shared logic ---

        private async Task<IActionResult> ProcessBookingAsync(string userId, int sessionId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return BadRequest("User not found.");

            var session = await _context.Sessions
                .Include(s => s.Class)
                .Include(s => s.Room)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session is null)
                return BadRequest("Session not found.");

            if (session.Bookings.Count >= session.MaxCapacity)
                return Conflict("This session is fully booked.");

            var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.SessionId == sessionId && b.UserId == userId);

            if (alreadyBooked)
                return Conflict("User has already booked this session.");

            var booking = new Booking
            {
                BookDate = DateTime.UtcNow,
                SessionId = session.SessionId,
                Session = session,
                UserId = user.Id,
                User = user
            };

            _context.Bookings.Add(booking);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to create booking.");

            await _outputCache.EvictByTagAsync("bookings", default);

            return CreatedAtAction(nameof(Get), new { id = booking.BookingId }, new BookingDTO
            {
                BookingId = booking.BookingId,
                BookDate = booking.BookDate,
                SessionId = session.SessionId,
                SessionStart = session.Start,
                SessionEnd = session.End,
                Subject = session.Class.Subject,
                RoomNumber = session.Room.RoomNumber,
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}"
            });
        }
    }
}
