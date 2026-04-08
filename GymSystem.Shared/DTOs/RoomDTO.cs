using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Data transfer object representing a room within a gym branch.
public class RoomDTO
{
    // Unique room identifier.
    public int RoomId { get; set; }

    // Room number within the branch (1–100).
    [Range(1, 100)]
    public int RoomNumber { get; set; }

    // Foreign key to the branch this room belongs to.
    public int BranchId { get; set; }

    // Maximum occupancy of the room (1–100).
    [Range(1, 100)]
    public int MaxCapacity { get; set; }

    // Number of sessions currently scheduled in this room.
    public int SessionCount { get; set; }

    // Number of equipment items assigned to this room.
    public int EquipmentCount { get; set; }
}
