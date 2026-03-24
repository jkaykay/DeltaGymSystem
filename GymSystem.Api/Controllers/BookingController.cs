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
    public class BookingController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(GymDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private IQueryable<Booking> BookingsQuery()
        {
            return _context.Bookings
                .Include(b => b.Session)
                    .ThenInclude(s => s.Class)
                .Include(b => b.Session)
                    .ThenInclude(s => s.Room)
                .Include(b => b.User);
        }

        private static BookingDTO ToDto(Booking b) => new()
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
        };

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await BookingsQuery().ToListAsync();
            var result = bookings.Select(ToDto).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Get(int id)
        {
            var booking = await BookingsQuery()
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return Ok(ToDto(booking));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var bookings = await BookingsQuery()
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var result = bookings.Select(ToDto).ToList();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddBookingRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var session = await _context.Sessions
                .Include(s => s.Class)
                .Include(s => s.Room)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SessionId == request.SessionId);

            if (session == null)
            {
                return BadRequest("Session not found.");
            }

            if (session.Bookings.Count >= session.MaxCapacity)
            {
                return Conflict("This session is fully booked.");
            }

            var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.SessionId == request.SessionId && b.UserId == request.UserId);

            if (alreadyBooked)
            {
                return Conflict("User has already booked this session.");
            }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to delete booking.");

            return NoContent();
        }

        [HttpGet("total")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _context.Bookings.CountAsync();
            return Ok(new CountResponse { Count = total });
        }
    }
}