using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Tier
    {
        [Key]
        [MaxLength(25)]
        public required string TierName { get; set; }
        public decimal Price { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
