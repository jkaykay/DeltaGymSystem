using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    public class SessionDTO
    {
        public int SessionId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int ClassId { get; set; }

        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        public int RoomId { get; set; }
        public int RoomNumber { get; set; }

        [Range(1, 100)]
        public int MaxCapacity { get; set; }

        public int BookingCount { get; set; }

        [MaxLength(450)]
        public string InstructorId { get; set; } = string.Empty;

        [MaxLength(201)]
        public string InstructorName { get; set; } = string.Empty;

        public int AvailableSpots => MaxCapacity - BookingCount;
    }
}
