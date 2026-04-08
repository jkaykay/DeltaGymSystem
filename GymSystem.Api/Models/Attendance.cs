// ============================================================
// Attendance.cs — Tracks a member's gym visit (check-in / check-out).
// When a member checks in, a new Attendance record is created with
// InFlag = true. When they check out, CheckOut is set and InFlag
// becomes false. The AutoCheckoutService handles forgotten check-outs.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }       // Auto-generated primary key
        public DateTime CheckIn { get; set; }       // When the member arrived
        public DateTime? CheckOut { get; set; }     // When they left (null if still inside)
        public bool InFlag { get; set; }            // True = currently in the gym

        // Foreign key to the Member
        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }
    }
}
