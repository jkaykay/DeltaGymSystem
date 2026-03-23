namespace GymSystem.Shared.DTOs;

public record UpdateAttendanceRequest(
    DateTime? CheckIn = null,
    DateTime? CheckOut = null,
    string? UserId = null,
    bool? InFlag = null
    );
