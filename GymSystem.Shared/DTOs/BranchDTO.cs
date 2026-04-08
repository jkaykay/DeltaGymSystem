using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs
{
    // Data transfer object representing a gym branch location.
    public class BranchDTO
    {
        // Unique branch identifier.
        public int BranchId { get; set; }

        // Street address of the branch (required, max 200 characters).
        [Required, MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        // City where the branch is located (required, max 100 characters).
        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        // Province or state of the branch (required, max 100 characters).
        [Required, MaxLength(100)]
        public string Province { get; set; } = string.Empty;

        // Postal code of the branch (required, max 10 characters).
        [Required, MaxLength(10)]
        public string PostCode { get; set; } = string.Empty;

        // Date the branch was opened.
        public DateTime OpenDate { get; set; }
    }
}
