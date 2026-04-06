using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Trainer.ViewModels
{
    public class TrainerDashboardViewModel
    {
        public string TrainerName { get; set; } = "";
        public List<SessionDTO> TodaySessions { get; set; } = new();
        public List<SessionDTO> UpcomingSessions { get; set; } = new();
    }
}