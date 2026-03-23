using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateSessionRequest(
    DateTime? Start = null,
    DateTime? End = null,
    int? RoomId = null,
    [Range(1, 1000)] int? MaxCapacity = null
);