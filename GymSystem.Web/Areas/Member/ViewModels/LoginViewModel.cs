using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Member.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or username is required.")]
        [Display(Name = "Email or Username")]
        public string EmailOrUserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}