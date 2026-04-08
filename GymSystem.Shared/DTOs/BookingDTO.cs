using System;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    // Data transfer object representing a session booking by a member.
    public class BookingDTO
    {
        // Unique booking identifier.
        public int BookingId { get; set; }

        // Date and time the booking was created.
        public DateTime BookDate { get; set; }

        // Foreign key to the booked session.
        public int SessionId { get; set; }

        // Start date and time of the booked session.
        public DateTime SessionStart { get; set; }

        // End date and time of the booked session.
        public DateTime SessionEnd { get; set; }

        // Subject name of the booked class (max 100 characters).
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        // Room number where the session takes place.
        public int RoomNumber { get; set; }

        // Identity identifier of the member who booked (max 450 characters).
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        // Display name of the member who booked (max 201 characters).
        [MaxLength(201)]
        public string UserName { get; set; } = string.Empty;
    }
}
