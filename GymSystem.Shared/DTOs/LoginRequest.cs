using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for authenticating a user via email/username and password.
// EmailOrUserName: Email or username credential (required, max 256 characters).
// Password: User password (required).
//create a record
public record LoginRequest(
    [Required, MaxLength(256)] string EmailOrUserName, //contain atleast one value
    [Required] string Password
);
