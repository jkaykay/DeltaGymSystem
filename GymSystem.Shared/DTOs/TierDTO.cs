using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    public class TierDTO
    {
        [Required, MaxLength(25)]
        public string TierName { get; set; } = string.Empty;

        [Range(0.01, 99999.99)]
        public decimal Price { get; set; }

        public int SubCount { get; set; }
    }
}
