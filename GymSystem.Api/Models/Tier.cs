// ============================================================
// Tier.cs — Represents a membership tier (e.g. "Gold", "Silver").
// Each tier has a name (used as the primary key) and a monthly price.
// Members subscribe to a tier to gain gym access.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Tier
    {
        [Key]                       // TierName is the primary key (not an auto-incremented int)
        [MaxLength(25)]
        public required string TierName { get; set; }

        [Range(0.01, 99999.99)]     // Price must be between 0.01 and 99999.99
        public decimal Price { get; set; }

        // All subscriptions that belong to this tier
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
