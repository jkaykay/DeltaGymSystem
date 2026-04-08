using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for creating a new gym class.
// Subject: Class subject or name (required, max 100 characters).
// UserId: Identity identifier of the trainer assigned to this class (required, max 450 characters).
public record AddClassRequest(
    [Required, MaxLength(100)] string Subject,
    [Required, MaxLength(450)] string UserId
);
