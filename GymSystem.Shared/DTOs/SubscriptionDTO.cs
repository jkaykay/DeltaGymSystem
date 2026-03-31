using System.ComponentModel.DataAnnotations;
using GymSystem.Shared.Enums;

namespace GymSystem.Shared.DTOs;

public class SubscriptionDTO
{
    public int SubId { get; set; }
    public SubscriptionState State { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [Required, MaxLength(25)]
    public string TierName { get; set; } = string.Empty;

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(201)]
    public string MemberName { get; set; } = string.Empty;
}