using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddMyPaymentRequest(
    [Required, Range(0.01, 99999.99)] decimal Amount,
    [Required] int SubId
);