using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditStaffViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
    }
}
