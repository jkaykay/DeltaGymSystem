using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating a branch location. Null properties are left unchanged.
// Address: Updated street address (max 200 characters, null to keep current).
// City: Updated city (max 100 characters, null to keep current).
// Province: Updated province or state (max 100 characters, null to keep current).
// PostCode: Updated postal code (max 10 characters, null to keep current).
public record UpdateBranchRequest(
    [MaxLength(200)] string? Address = null,
    [MaxLength(100)] string? City = null,
    [MaxLength(100)] string? Province = null,
    [MaxLength(10)] string? PostCode = null
);
