using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for a trainer to update their own profile. Null properties are left unchanged.
// Email: Updated email address (max 256 characters, null to keep current).
// FirstName: Updated first name (max 100 characters, null to keep current).
// LastName: Updated last name (max 100 characters, null to keep current).
// PhoneNumber: Updated phone number (max 20 characters, null to keep current).
public record UpdateTrainerProfileRequest(
    [EmailAddress, MaxLength(256)] string? Email = null,
    [MaxLength(100)] string? FirstName = null,
    [MaxLength(100)] string? LastName = null,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);
