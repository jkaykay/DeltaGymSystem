namespace GymSystem.Shared.DTOs;

public record UpdateClassRequest(
    string? Subject = null,
    string? UserId = null
);