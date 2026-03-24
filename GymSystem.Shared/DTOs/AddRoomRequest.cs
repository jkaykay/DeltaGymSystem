using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddRoomRequest(
    [Required, Range(1, 100)] int RoomNumber,
    [Required] int BranchId,
    [Required, Range(1, 100)] int MaxCapacity
);