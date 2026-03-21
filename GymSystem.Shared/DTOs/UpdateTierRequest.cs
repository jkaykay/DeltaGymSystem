namespace GymSystem.Shared.DTOs;

public record UpdateTierRequest
    (
    string? TierName,
    decimal? Price
    );