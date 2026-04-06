using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record CreateStaffRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [MaxLength(50)] string? EmployeeId,
    [Required, RegularExpression("^(Admin|Staff)$", ErrorMessage = "Role must be 'Admin' or 'Staff'.")] string Role,
    int? BranchId,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);