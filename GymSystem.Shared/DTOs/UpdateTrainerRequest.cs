namespace GymSystem.Shared.DTOs;

public record UpdateTrainerRequest(
    string? Email = null,
    string? FirstName = null,
    string? LastName = null,
    string? EmployeeId = null,
    int? BranchId = null
);