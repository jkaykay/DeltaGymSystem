using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateSessionRequest(
    DateTime? Start = null,
    DateTime? End = null,
    int? RoomId = null,
    [Range(1, 100)] int? MaxCapacity = null
);