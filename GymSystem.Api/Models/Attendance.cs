using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public bool InFlag { get; set; }

        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }
    }
}
