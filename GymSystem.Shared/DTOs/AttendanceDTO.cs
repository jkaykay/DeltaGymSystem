using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    public class AttendanceDTO
    {
        public int AttendanceId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        [Required, MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(201)]
        public string MemberName { get; set; } = string.Empty;

        public bool InFlag { get; set; }
    }
}
