using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for scanning a member QR code at the gym entrance.
// Token: The QR token string to validate (required).
public record ScanRequest(
    [Required] string Token
);
