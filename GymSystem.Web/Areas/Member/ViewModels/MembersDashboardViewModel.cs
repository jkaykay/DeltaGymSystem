using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Member.ViewModels
{
    public class DashboardViewModel
    {
        public string Username { get; set; } = "";
        public int UpcomingClasses { get; set; }
        public int TotalBookings { get; set; }
        public int ClassesAttended { get; set; }
        public List<AttendanceDTO> LogHistory { get; set; } = [];
        public List<PaymentDTO> PaymentHistory { get; set; } = [];
    }
}