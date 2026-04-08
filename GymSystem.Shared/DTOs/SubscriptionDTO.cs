using System.ComponentModel.DataAnnotations;
using GymSystem.Shared.Enums;

namespace GymSystem.Shared.DTOs;

// Data transfer object representing a member subscription.
public class SubscriptionDTO
{
    // Unique subscription identifier.
    public int SubId { get; set; }

    // Current state of the subscription (Pending, Active, Queued, Expired).
    public SubscriptionState State { get; set; }

    // Start date of the subscription period.
    public DateTime StartDate { get; set; }

    // End date of the subscription period.
    public DateTime EndDate { get; set; }

    // Name of the subscription tier (required, max 25 characters).
    [Required, MaxLength(25)]
    public string TierName { get; set; } = string.Empty;

    // Identity identifier of the subscribing member (required, max 450 characters).
    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    // Display name of the subscribing member (max 201 characters).
    [MaxLength(201)]
    public string MemberName { get; set; } = string.Empty;
}
