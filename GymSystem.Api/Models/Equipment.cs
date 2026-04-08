// ============================================================
// Equipment.cs — Represents a piece of gym equipment.
// Each equipment item belongs to a room and has a description,
// the date it was brought in, and whether it's operational.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }        // Auto-generated primary key

        [MaxLength(500)]
        public string? Description { get; set; }    // e.g. "Treadmill", "Bench Press"
        public required DateTime InDate { get; set; } // When the equipment was added
        public bool Operational { get; set; }       // True if working, false if broken

        // Foreign key to the Room where this equipment is located
        public required int RoomId { get; set; }
        public required Room Room { get; set; }
    }
}
