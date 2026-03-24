using System.ComponentModel.DataAnnotations;
using GymSystem.Shared.Enums;

namespace GymSystem.Shared.DTOs;

public record UpdateSubscriptionRequest(
    [MaxLength(25)] string? TierName = null,
    SubscriptionState? State = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);