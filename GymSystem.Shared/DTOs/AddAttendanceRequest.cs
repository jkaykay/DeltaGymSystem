namespace GymSystem.Shared.DTOs;

public record AddAttendanceRequest(
    DateTime CheckIn,
    string UserId
    );
