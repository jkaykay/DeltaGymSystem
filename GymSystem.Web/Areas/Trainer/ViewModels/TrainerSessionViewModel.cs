using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Trainer.ViewModels
{
    public class TrainerSessionViewModel
    {
        public string TrainerName { get; set; } = "";
        public List<SessionDTO> WeeklySessions { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}