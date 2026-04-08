using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for a member to create a subscription for themselves.
// TierName: Name of the subscription tier to subscribe to (required, max 25 characters).
public record AddMySubscriptionRequest(
    [Required, MaxLength(25)] string TierName
);
