namespace GymSystem.Api.Models
{
    public class Class
    {
        public int ClassId { get; set; }
        public string Subject { get; set; } = string.Empty;
        
        //FK
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
