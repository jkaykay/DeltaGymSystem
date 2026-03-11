using GymSystem.Web.DTOs.Management;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalMembers { get; set; } = 0;
        public int TotalStaff { get; set; } = 0;
        public List<MemberResponse> RecentSignups { get; set; } = new();
    }
}
