using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for an admin or staff to create a booking on behalf of a member.
// SessionId: Foreign key to the session to book (required).
// UserId: Identity identifier of the member to book for (required, max 450 characters).
public record AddBookingRequest(
    [Required] int SessionId,
    [Required, MaxLength(450)] string UserId
);
