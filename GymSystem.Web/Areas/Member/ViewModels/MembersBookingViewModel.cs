using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Member.ViewModels;

public class BookingViewModel
{
    public List<SessionDTO> Sessions { get; set; } = [];
    public List<BookingDTO> MyBookings { get; set; } = [];
}
