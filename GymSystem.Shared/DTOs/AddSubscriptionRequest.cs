using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for creating a new subscription for a member (admin/staff use).
// TierName: Name of the subscription tier (required, max 25 characters).
// UserId: Identity identifier of the subscribing member (required, max 450 characters).
public record AddSubscriptionRequest(
    [Required, MaxLength(25)] string TierName,
    [Required, MaxLength(450)] string UserId
);
