using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateRoomRequest(
    [Range(1, int.MaxValue)] int? RoomNumber = null,
    int? BranchId = null,
    [Range(1, 1000)] int? MaxCapacity = null
);