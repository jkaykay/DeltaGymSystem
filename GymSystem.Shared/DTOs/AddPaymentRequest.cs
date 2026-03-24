using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddPaymentRequest(
    [Required, Range(0.01, 99999.99)] decimal Amount,
    [Required, MaxLength(450)] string UserId,
    [Required] int SubId
);