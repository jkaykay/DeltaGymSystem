using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Trainer.ViewModels
{
    public class SessionDetailViewModel
    {
        public SessionDTO Session { get; set; } = new();
        public List<BookingDTO> Bookings { get; set; } = new();
    }
}
