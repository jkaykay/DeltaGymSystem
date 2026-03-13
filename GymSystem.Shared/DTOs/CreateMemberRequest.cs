namespace GymSystem.Shared.DTOs;

public record CreateMemberRequest(
    string Username, 
    string Email, 
    string FirstName, 
    string LastName,
    string Password
);