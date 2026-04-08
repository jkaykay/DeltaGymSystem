// ============================================================
// Branch.cs — Represents a physical gym location.
// Each branch has an address and can contain multiple rooms
// and be associated with multiple users (staff, trainers).
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Branch
    {
        public int BranchId { get; set; }           // Auto-generated primary key

        [MaxLength(200)]
        public required string Address { get; set; }

        [MaxLength(100)]
        public required string City { get; set; }

        [MaxLength(100)]
        public required string Province { get; set; }

        [MaxLength(10)]
        public required string PostCode { get; set; }

        public DateTime OpenDate { get; set; } = DateTime.UtcNow; // When this branch opened

        // Navigation collections — rooms in this branch and users assigned here
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
