using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    // Data transfer object representing a member attendance record.
    public class AttendanceDTO
    {
        // Unique attendance record identifier.
        public int AttendanceId { get; set; }

        // Date and time the member checked in.
        public DateTime CheckIn { get; set; }

        // Date and time the member checked out (null if still checked in).
        public DateTime? CheckOut { get; set; }

        // Identity identifier of the member (required, max 450 characters).
        [Required, MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        // Display name of the member (max 201 characters).
        [MaxLength(201)]
        public string MemberName { get; set; } = string.Empty;

        // Flag indicating whether the member is currently checked in.
        public bool InFlag { get; set; }
    }
}
