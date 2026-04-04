using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Trainer.ViewModels
{
    public class TrainerScheduleViewModel
    {
        public string TrainerName { get; set; } = "";
        public List<SessionDTO> WeeklySessions { get; set; } = new();
    }
}