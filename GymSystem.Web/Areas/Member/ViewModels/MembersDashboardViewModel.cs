using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Member.ViewModels
{
    public class DashboardViewModel
    {
        public string Username { get; set; } = "";
        public int TotalBookings { get; set; }
        public int TotalAttendances { get; set; }
        public List<BookingDTO> UpcomingBookings { get; set; } = [];
        public List<AttendanceDTO> LogHistory { get; set; } = [];
        public List<PaymentDTO> PaymentHistory { get; set; } = [];
    }
}