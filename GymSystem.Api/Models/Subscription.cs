using GymSystem.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Subscription
    {
        [Key]
        public int SubId { get; set; }
        public SubscriptionState State { get; set; } = SubscriptionState.Pending;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        //FKs
        public required string TierName { get; set; }
        public required Tier Tier { get; set; }

        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}