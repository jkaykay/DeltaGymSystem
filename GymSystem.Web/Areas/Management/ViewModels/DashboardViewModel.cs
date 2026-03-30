using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalMembers { get; set; } = 0;
        public int TotalStaff { get; set; } = 0;
        public int TotalTrainers { get; set; } = 0;
        public List<UserDTO> RecentSignups { get; set; } = new();
    }
}
