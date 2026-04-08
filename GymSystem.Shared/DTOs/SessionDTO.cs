using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    // Data transfer object representing a scheduled class session.
    public class SessionDTO
    {
        // Unique session identifier.
        public int SessionId { get; set; }

        // Session start date and time.
        public DateTime Start { get; set; }

        // Session end date and time.
        public DateTime End { get; set; }

        // Foreign key to the class this session belongs to.
        public int ClassId { get; set; }

        // Subject name of the associated class (max 100 characters).
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        // Foreign key to the room where the session is held.
        public int RoomId { get; set; }

        // Room number where the session is held (resolved server-side).
        public int RoomNumber { get; set; }

        // Maximum number of bookings allowed for this session (1–100).
        [Range(1, 100)]
        public int MaxCapacity { get; set; }

        // Current number of bookings made for this session.
        public int BookingCount { get; set; }

        // Identity identifier of the instructor (max 450 characters).
        [MaxLength(450)]
        public string InstructorId { get; set; } = string.Empty;

        // Display name of the instructor (max 201 characters).
        [MaxLength(201)]
        public string InstructorName { get; set; } = string.Empty;

        // Calculated number of remaining spots (MaxCapacity minus BookingCount).
        public int AvailableSpots => MaxCapacity - BookingCount;
    }
}
