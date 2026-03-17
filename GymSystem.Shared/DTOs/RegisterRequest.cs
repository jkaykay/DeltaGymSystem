namespace GymSystem.Shared.DTOs;

public record RegisterRequest(string UserName, string Email, string Password, string FirstName, string LastName);
