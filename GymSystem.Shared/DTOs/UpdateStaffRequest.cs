namespace GymSystem.Shared.DTOs;

public record UpdateStaffRequest(
    string? FirstName = null,
    string? LastName = null,
    string? EmployeeId = null,
    bool? Active = null,
    int? BranchId = null
);