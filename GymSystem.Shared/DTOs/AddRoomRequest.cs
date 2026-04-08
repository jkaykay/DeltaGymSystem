using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for creating a new room in a branch.
// RoomNumber: Room number within the branch (required, 1–100).
// BranchId: Foreign key to the branch (required).
// MaxCapacity: Maximum occupancy of the room (required, 1–100).
public record AddRoomRequest(
    [Required, Range(1, 100)] int RoomNumber,
    [Required] int BranchId,
    [Required, Range(1, 100)] int MaxCapacity
);
