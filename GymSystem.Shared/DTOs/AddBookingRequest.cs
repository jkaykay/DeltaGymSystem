using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddBookingRequest(
    [Required] int SessionId,
    [Required, MaxLength(450)] string UserId
);