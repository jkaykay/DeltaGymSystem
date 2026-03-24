using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddSubscriptionRequest(
    [Required, MaxLength(25)] string TierName,
    [Required, MaxLength(450)] string UserId
);