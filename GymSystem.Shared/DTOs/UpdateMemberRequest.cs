namespace GymSystem.Shared.DTOs;

public record UpdateMemberRequest(
    string? FirstName = null,
    string? LastName = null,
    bool? Active = null
);