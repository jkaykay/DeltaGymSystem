using GymSystem.Shared.Enums;

namespace GymSystem.Shared.DTOs;

public record UpdateSubscriptionRequest(
    string? TierName,
    SubscriptionState? State,
    DateTime? StartDate,
    DateTime? EndDate
    );