namespace GymSystem.Shared.DTOs;

public record UpdateTrainerRequest 
(
    string? Email,
    string? FirstName,
    string? LastName,
    string? EmployeeId
);
