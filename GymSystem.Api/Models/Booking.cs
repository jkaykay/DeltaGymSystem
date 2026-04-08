// ============================================================
// Booking.cs — Represents a member's reservation for a session.
// A booking links one user to one session. The unique index in
// GymDbContext ensures a user cannot book the same session twice.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Booking
    {
        public int BookingId { get; set; }          // Auto-generated primary key
        public DateTime BookDate { get; set; }      // When the booking was made

        // Foreign key to the Session being booked
        public required int SessionId { get; set; }
        public required Session Session { get; set; }

        // Foreign key to the Member who made the booking
        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }
    }
}
