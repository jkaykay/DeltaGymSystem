using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Member.ViewModels;

public class PaymentHistoryViewModel
{
    public List<PaymentDTO> Payments { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
