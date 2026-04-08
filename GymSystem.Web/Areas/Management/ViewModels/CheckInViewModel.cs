using System.ComponentModel.DataAnnotations;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class CheckInViewModel
    {
        [Required]
        [Display(Name = "Member")]
        public string UserId { get; set; } = string.Empty;

        public List<UserDTO> Members { get; set; } = [];
    }
}