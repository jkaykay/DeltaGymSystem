namespace GymSystem.Shared.DTOs;

//send these data from api to the web app
public record LoginResponse(
    string Token,
    string Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string GymLocation,
    List<string> Roles
);