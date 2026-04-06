using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using GymSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
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
        public async Task<IActionResult> GetAll([FromQuery] BookingSearchRequest request)
        {
            var query = _context.Bookings.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();
                query = query.Where(b =>
                    b.User.FirstName.ToLower().Contains(term) ||
                    b.User.LastName.ToLower().Contains(term) ||
                    b.Session.Class.Subject.ToLower().Contains(term));
            }

            if (request.DateFrom.HasValue)
                query = query.Where(b => b.Session.Start >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(b => b.Session.Start <= request.DateTo.Value);

            var descending = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = request.SortBy?.ToLower() switch
            {
                "member"  => descending ? query.OrderByDescending(b => b.User.LastName)       : query.OrderBy(b => b.User.LastName),
                "subject" => descending ? query.OrderByDescending(b => b.Session.Class.Subject) : query.OrderBy(b => b.Session.Class.Subject),
                "session" => descending ? query.OrderByDescending(b => b.Session.Start)       : query.OrderBy(b => b.Session.Start),
                _         => descending ? query.OrderByDescending(b => b.BookDate)            : query.OrderBy(b => b.BookDate),
            };

            var result = await query
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
                .ToPagedResultAsync(request.Page, request.PageSize);

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
        public async Task<IActionResult> GetByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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
                .ToPagedResultAsync(page, pageSize);

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
        public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var query = _context.Bookings
                .Where(b => b.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(b =>
                    b.Session.Class.Subject.ToLower().Contains(term));
            }

            var result = await query
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
                .ToPagedResultAsync(page, pageSize);

            return Ok(result);
        }

        [HttpPost("my")]
        [Authorize(Roles = "Member")]  // UserId comes from claims — members can only book for themselves
        [EnableRateLimiting("booking")]
        public async Task<IActionResult> CreateMy([FromBody] AddMyBookingRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return await ProcessBookingAsync(userId, request.SessionId);
        }

        [HttpDelete("my/{id}")]
        [Authorize(Roles = "Member")]
        [EnableRateLimiting("booking")]
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

            if (session.Start <= DateTime.UtcNow)
                return BadRequest("Cannot book a session that has already started.");

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

            try
            {
                var rowsAffected = await _context.SaveChangesAsync();
                if (rowsAffected == 0) return BadRequest("Failed to create booking.");
            }
            catch (DbUpdateException)
            {
                return Conflict("User has already booked this session.");
            }

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