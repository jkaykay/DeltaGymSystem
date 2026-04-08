using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating a member's details. Null properties are left unchanged.
// Email: Updated email address (max 256 characters, null to keep current).
// FirstName: Updated first name (max 100 characters, null to keep current).
// LastName: Updated last name (max 100 characters, null to keep current).
// QrCodeBase64: Updated QR code as a base64-encoded string (max 256 characters, null to keep current).
// IsActive: Updated active status (null to keep current).
// PhoneNumber: Updated phone number (max 20 characters, null to keep current).
public record UpdateMemberRequest(
    [EmailAddress, MaxLength(256)] string? Email = null,
    [MaxLength(100)] string? FirstName = null,
    [MaxLength(100)] string? LastName = null,
    [MaxLength(256)] string? QrCodeBase64 = null,
    bool? IsActive = null,
    [Phone, MaxLength(20)] string? PhoneNumber = null
);
