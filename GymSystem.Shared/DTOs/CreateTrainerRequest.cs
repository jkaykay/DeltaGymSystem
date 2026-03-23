namespace GymSystem.Shared.DTOs;

public record CreateTrainerRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? EmployeeId,
    int? BranchId
);