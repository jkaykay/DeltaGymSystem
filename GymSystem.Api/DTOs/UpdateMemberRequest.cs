namespace GymSystem.Api.DTOs;

public record UpdateMemberRequest(string FirstName, string LastName, bool Active);