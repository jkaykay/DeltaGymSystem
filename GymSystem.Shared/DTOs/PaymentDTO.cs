using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    // Data transfer object representing a payment transaction.
    public class PaymentDTO
    {
        // Unique payment identifier.
        public int PaymentId { get; set; }

        // Payment amount (0.01–99999.99).
        [Range(0.01, 99999.99)]
        public decimal Amount { get; set; }

        // Date and time the payment was made.
        public DateTime PaymentDate { get; set; }

        // Identity identifier of the paying user (required, max 450 characters).
        [Required, MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        // Full name of the paying user (resolved server-side).
        public string? UserFullName { get; set; }

        // Foreign key to the subscription this payment applies to.
        public int SubId { get; set; }
    }
}
