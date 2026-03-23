namespace GymSystem.Shared.DTOs;

public record CreateStaffRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? EmployeeId,
    string Role,
    int? BranchId);