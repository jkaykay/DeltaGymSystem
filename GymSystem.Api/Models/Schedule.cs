// ============================================================
// Schedule.cs — Represents a work schedule entry for staff or trainers.
// Each entry has a start time, end time, and the user it belongs to.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }         // Auto-generated primary key
        public DateTime Start { get; set; }         // Shift start time
        public DateTime End { get; set; }           // Shift end time

        // Foreign key to the Staff/Trainer this schedule is for
        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }
    }
}
