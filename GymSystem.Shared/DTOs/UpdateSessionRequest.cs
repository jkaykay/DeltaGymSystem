namespace GymSystem.Shared.DTOs;

public record UpdateSessionRequest(
    DateTime? Start = null,
    DateTime? End = null,
    int? RoomId = null,
    int? MaxCapacity = null
);
