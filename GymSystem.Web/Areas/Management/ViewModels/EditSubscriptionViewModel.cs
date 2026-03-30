using GymSystem.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Management.ViewModels
{
    public class EditSubscriptionViewModel
    {
        public int SubId { get; set; }

        // Read-only display context
        public string MemberName { get; set; } = string.Empty;

        [MaxLength(25)]
        [Display(Name = "Tier")]
        public string? TierName { get; set; }

        [Display(Name = "State")]
        public SubscriptionState? State { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }
    }
}