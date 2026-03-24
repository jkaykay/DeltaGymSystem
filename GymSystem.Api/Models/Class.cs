using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Class
    {
        public int ClassId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        //FK
        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
