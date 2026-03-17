using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalMembers { get; set; } = 0;
        public int TotalStaff { get; set; } = 0;
        public List<UserDto> RecentSignups { get; set; } = new();
    }
}
