using System.ComponentModel.DataAnnotations;
using GymSystem.Shared.Enums;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating a subscription. Null properties are left unchanged.
// TierName: Updated tier name (max 25 characters, null to keep current).
// State: Updated subscription state (null to keep current).
// StartDate: Updated start date (null to keep current).
// EndDate: Updated end date (null to keep current).
public record UpdateSubscriptionRequest(
    [MaxLength(25)] string? TierName = null,
    SubscriptionState? State = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
