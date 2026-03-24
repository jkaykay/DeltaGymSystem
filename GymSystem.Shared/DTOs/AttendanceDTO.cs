namespace GymSystem.Shared.DTOs
{
    public class AttendanceDTO
    {
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public bool InFlag { get; set; }
    }
}
