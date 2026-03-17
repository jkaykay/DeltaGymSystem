namespace GymSystem.Shared.DTOs;

public record LoginResponse(
    string Token, string Id, String UserName, string Email, string FirstName, string LastName, List<string> Roles);
