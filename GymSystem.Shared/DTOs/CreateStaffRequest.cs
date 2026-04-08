using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for creating a new staff member account.
// Email: Email address (required, validated format, max 256 characters).
// FirstName: First name (required, max 100 characters).
// LastName: Last name (required, max 100 characters).
// Password: Initial password (required, 6–100 characters).
// EmployeeId: Optional employee identifier (max 50 characters).
// Role: Role to assign: must be 'Admin' or 'Staff' (required).
// BranchId: Optional branch assignment.
// PhoneNumber: Optional phone number (max 20 characters).
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
