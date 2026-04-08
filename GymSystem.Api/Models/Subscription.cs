// ============================================================
// Subscription.cs — Represents a member's subscription to a tier.
// A subscription starts as "Pending" (awaiting payment), becomes
// "Active" once paid, may be "Queued" for future activation, and
// eventually becomes "Expired" when its end date passes.
// ============================================================

using GymSystem.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Subscription
    {
        [Key]
        public int SubId { get; set; }                                  // Auto-generated primary key
        public SubscriptionState State { get; set; } = SubscriptionState.Pending; // Current status
        public DateTime StartDate { get; set; }                         // When the subscription begins
        public DateTime EndDate { get; set; }                           // When the subscription expires

        // Foreign keys — link this subscription to a tier and a user
        public required string TierName { get; set; }
        public required Tier Tier { get; set; }                         // Navigation to the Tier

        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }              // Navigation to the User

        // Payments made against this subscription
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}