using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddSessionRequest(
    [Required] DateTime Start,
    [Required] DateTime End,
    [Required] int RoomId,
    [Required] int ClassId,
    [Required, Range(1, 100)] int MaxCapacity
);