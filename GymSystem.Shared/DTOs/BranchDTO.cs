using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    public class BranchDTO
    {
        public int BranchId { get; set; }

        [Required, MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Province { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string PostCode { get; set; } = string.Empty;

        public DateTime OpenDate { get; set; }
    }
}
