using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateTrainerRequest(
    [EmailAddress, MaxLength(256)] string? Email = null,
    [MaxLength(100)] string? FirstName = null,
    [MaxLength(100)] string? LastName = null,
    [MaxLength(50)] string? EmployeeId = null,
    int? BranchId = null,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);