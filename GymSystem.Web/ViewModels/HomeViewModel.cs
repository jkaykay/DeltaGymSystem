using GymSystem.Shared.DTOs;

namespace GymSystem.Web.ViewModels
{
    public class HomeViewModel
    {
        public List<SessionDTO> Sessions { get; set; } = [];
        public List<TierDTO> Tiers { get; set; } = [];
        public List<UserDTO> Trainers { get; set; } = [];
    }
}
