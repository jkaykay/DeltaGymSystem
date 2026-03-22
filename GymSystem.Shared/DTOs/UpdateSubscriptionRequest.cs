namespace GymSystem.Shared.DTOs;

public record UpdateSubscriptionRequest(
    string? TierName,
    bool? Status,
    DateTime? StartDate,
    DateTime? EndDate
    );