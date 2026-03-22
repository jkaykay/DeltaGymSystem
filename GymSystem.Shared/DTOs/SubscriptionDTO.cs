namespace GymSystem.Shared.DTOs;

public class SubscriptionDTO
{
    public int SubId { get; set; }
    public bool Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TierName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
}