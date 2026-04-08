namespace GymSystem.Shared.DTOs;

// Response DTO returned after successful authentication.
// Contains a JWT token and user profile information.
// Token: JWT authentication token.
// Id: Unique identity identifier of the authenticated user.
// UserName: Username of the authenticated user.
// Email: Email address of the authenticated user.
// FirstName: First name of the authenticated user.
// LastName: Last name of the authenticated user.
// GymLocation: Gym branch location associated with the user.
// Roles: List of roles assigned to the authenticated user.
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
