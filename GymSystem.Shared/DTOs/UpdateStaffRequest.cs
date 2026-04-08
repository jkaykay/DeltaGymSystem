using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating a staff member's details. Null properties are left unchanged.
// Email: Updated email address (max 256 characters, null to keep current).
// FirstName: Updated first name (max 100 characters, null to keep current).
// LastName: Updated last name (max 100 characters, null to keep current).
// EmployeeId: Updated employee identifier (max 50 characters, null to keep current).
// Active: Updated active status (null to keep current).
// BranchId: Updated branch assignment (null to keep current).
// PhoneNumber: Updated phone number (max 20 characters, null to keep current).
public record UpdateStaffRequest(
    [EmailAddress, MaxLength(256)] string? Email = null,
    [MaxLength(100)] string? FirstName = null,
    [MaxLength(100)] string? LastName = null,
    [MaxLength(50)] string? EmployeeId = null,
    bool? Active = null,
    int? BranchId = null,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);
