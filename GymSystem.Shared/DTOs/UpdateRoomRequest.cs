using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating a room. Null properties are left unchanged.
// RoomNumber: Updated room number (1–100, null to keep current).
// BranchId: Updated branch assignment (null to keep current).
// MaxCapacity: Updated maximum capacity (1–100, null to keep current).
public record UpdateRoomRequest(
    [Range(1, 100)] int? RoomNumber = null,
    int? BranchId = null,
    [Range(1, 100)] int? MaxCapacity = null
);
