namespace GymSystem.Shared.DTOs;

public record AddSessionRequest
(
    DateTime Start,
    DateTime End,
    int RoomId,
    int ClassId,
    int MaxCapacity
);
