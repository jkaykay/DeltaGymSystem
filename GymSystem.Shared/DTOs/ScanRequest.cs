using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record ScanRequest(
    [Required] string Token
);