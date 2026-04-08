using System.ComponentModel.DataAnnotations;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CreateBookingViewModel
    {
        [Required]
        [Display(Name = "Member")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Session")]
        public int SessionId { get; set; }

        public List<UserDTO> Members { get; set; } = [];
        public List<SessionDTO> Sessions { get; set; } = [];
    }
}