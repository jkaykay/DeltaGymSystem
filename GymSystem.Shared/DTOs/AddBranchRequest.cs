using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for creating a new gym branch location.
// Address: Street address (required, max 200 characters).
// City: City name (required, max 100 characters).
// Province: Province or state (required, max 100 characters).
// PostCode: Postal code (required, max 10 characters).
// OpenDate: Optional date the branch opened.
public record AddBranchRequest(
    [Required, MaxLength(200)] string Address,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(100)] string Province,
    [Required, MaxLength(10)] string PostCode,
    DateTime? OpenDate
);
