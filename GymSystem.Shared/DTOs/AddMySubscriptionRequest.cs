using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddMySubscriptionRequest(
    [Required, MaxLength(25)] string TierName
);