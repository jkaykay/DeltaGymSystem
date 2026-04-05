using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

//create a record
public record LoginRequest(
    [Required, MaxLength(256)] string EmailOrUserName, //contain atleast one value
    [Required] string Password
);