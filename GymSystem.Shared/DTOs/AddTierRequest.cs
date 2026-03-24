using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddTierRequest(
    [Required, MaxLength(25)] string TierName,
    [Required, Range(0.01, 99999.99)] decimal Price
);