namespace GymSystem.Api.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }


        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

    }
}
