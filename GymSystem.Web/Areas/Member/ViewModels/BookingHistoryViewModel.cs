using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Member.ViewModels;

public class BookingHistoryViewModel
{
    public string? Search { get; set; }
    public List<BookingDTO> Bookings { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
