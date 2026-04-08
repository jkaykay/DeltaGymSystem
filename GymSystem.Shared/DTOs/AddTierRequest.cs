using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for creating a new subscription tier.
// TierName: Unique tier name (required, max 25 characters).
// Price: Monthly price (required, 0.01–99999.99).
public record AddTierRequest(
    [Required, MaxLength(25)] string TierName,
    [Required, Range(0.01, 99999.99)] decimal Price
);
