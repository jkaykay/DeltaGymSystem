namespace GymSystem.Shared.DTOs;

public record AddSubscriptionRequest(
    string TierName,
    string UserId
    );