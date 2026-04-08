using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    // Data transfer object representing a subscription tier (pricing plan).
    public class TierDTO
    {
        // Name of the tier, used as the primary key (required, max 25 characters).
        [Required, MaxLength(25)]
        public string TierName { get; set; } = string.Empty;

        // Monthly price of the tier (0.01–99999.99).
        [Range(0.01, 99999.99)]
        public decimal Price { get; set; }

        // Number of active subscriptions on this tier.
        public int SubCount { get; set; }
    }
}
