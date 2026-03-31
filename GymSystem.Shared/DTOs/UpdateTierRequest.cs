using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record UpdateTierRequest(
    [Range(0.01, 99999.99)] decimal? Price = null
);