namespace GymSystem.Shared.DTOs;

public record AddRoomRequest(
    int RoomNumber,
    int BranchId,
    int MaxCapacity
);