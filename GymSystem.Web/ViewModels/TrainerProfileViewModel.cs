namespace GymSystem.Web.Areas.Trainer.Models
{
    public class TrainerProfileViewModel
    {
        public string FullName { get; set; } = "";
        public string RoleLabel { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string GymLocation { get; set; } = "";
        public bool IsEditing { get; set; }
    }
}