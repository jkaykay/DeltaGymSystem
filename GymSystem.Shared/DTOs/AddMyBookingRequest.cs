using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddMyBookingRequest(
    [Required] int SessionId
    );