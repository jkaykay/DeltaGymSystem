using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for recording a new payment against a subscription.
// Amount: Payment amount (required, 0.01–99999.99).
// UserId: Identity identifier of the paying member (required, max 450 characters).
// SubId: Foreign key to the subscription (required).
public record AddPaymentRequest(
    [Required, Range(0.01, 99999.99)] decimal Amount,
    [Required, MaxLength(450)] string UserId,
    [Required] int SubId
);
