using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddRoomRequest(
    [Required, Range(1, int.MaxValue)] int RoomNumber,
    [Required] int BranchId,
    [Required, Range(1, 1000)] int MaxCapacity
);