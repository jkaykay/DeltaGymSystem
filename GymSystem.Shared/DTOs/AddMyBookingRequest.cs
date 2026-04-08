using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for a member to book a session for themselves.
// SessionId: Foreign key to the session to book (required).
public record AddMyBookingRequest(
    [Required] int SessionId
    );
