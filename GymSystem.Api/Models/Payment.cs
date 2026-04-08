// ============================================================
// Payment.cs — Represents a financial payment made by a member.
// Each payment is linked to both a user and a subscription.
// Payments are immutable records — they cannot be edited after creation.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }          // Auto-generated primary key

        [Range(0.01, 99999.99)]                     // Amount must be positive
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }   // When the payment was made

        // Foreign key to the Member who paid
        [MaxLength(450)]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        // Foreign key to the Subscription this payment is for
        public required int SubId { get; set; }
        public required Subscription Subscription { get; set; }
    }
}
