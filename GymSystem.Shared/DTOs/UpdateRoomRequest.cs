namespace GymSystem.Shared.DTOs;

public record UpdateRoomRequest(
    int? RoomNumber,
    int? BranchId,
    int? MaxCapacity
);