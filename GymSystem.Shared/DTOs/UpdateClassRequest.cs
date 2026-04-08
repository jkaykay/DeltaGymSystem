using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for partially updating a gym class. Null properties are left unchanged.
// Subject: Updated class subject (max 100 characters, null to keep current).
// UserId: Updated trainer identity identifier (max 450 characters, null to keep current).
public record UpdateClassRequest(
    [MaxLength(100)] string? Subject = null,
    [MaxLength(450)] string? UserId = null
);
