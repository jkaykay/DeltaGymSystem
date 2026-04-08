using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for a member to make a payment on their own subscription.
// Amount: Payment amount (required, 0.01–99999.99).
// SubId: Foreign key to the member's subscription (required).
public record AddMyPaymentRequest(
    [Required, Range(0.01, 99999.99)] decimal Amount,
    [Required] int SubId
);
