using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateRoomRequest(
    [Range(1, 100)] int? RoomNumber = null,
    int? BranchId = null,
    [Range(1, 100)] int? MaxCapacity = null
);