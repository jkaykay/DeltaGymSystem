namespace GymSystem.Shared.DTOs;

public record AddTierRequest
    (
    string TierName,
    decimal Price
    );