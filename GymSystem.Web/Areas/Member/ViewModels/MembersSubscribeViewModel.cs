using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Member.ViewModels;

public class SubscribeViewModel
{
    public string MemberName { get; set; } = "";
    public string TierName { get; set; } = "";
    public decimal Price { get; set; }
}
