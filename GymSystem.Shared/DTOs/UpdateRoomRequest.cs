namespace GymSystem.Shared.DTOs;

public record UpdateRoomRequest(
    int? RoomNumber = null,
    int? BranchId = null,
    int? MaxCapacity = null
);