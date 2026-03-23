using System.ComponentModel.DataAnnotations;
using GymSystem.Shared.Enums;

namespace GymSystem.Shared.DTOs;

public record UpdateSubscriptionRequest(
    [MaxLength(25)] string? TierName,
    SubscriptionState? State,
    DateTime? StartDate,
    DateTime? EndDate
);