// ============================================================
// Room.cs — Represents a room inside a gym branch.
// Rooms host sessions and contain equipment. Each room has a
// number (unique within its branch) and a maximum capacity.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Room
    {
        public int RoomId { get; set; }             // Auto-generated primary key

        [Range(1, 100)]                             // Room number between 1 and 100
        public required int RoomNumber { get; set; }

        [Range(1, 100)]                             // How many people the room can hold
        public required int MaxCapacity { get; set; }

        // Foreign key to the Branch this room belongs to
        public required int BranchId { get; set; }
        public required Branch Branch { get; set; }

        // Navigation collections
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
