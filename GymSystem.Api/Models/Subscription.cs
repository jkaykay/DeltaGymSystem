using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Subscription
    {
        [Key]
        public int SubId { get; set; }
        public bool Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        //FKs
        public required string TierName { get; set; }
        public required Tier Tier { get; set; }

        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
