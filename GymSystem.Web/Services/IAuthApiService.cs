using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    // Interface for the authentication API service.
    // Defines the contract for login, logout, and password-change operations.
    // Any class that implements this interface must provide these methods.
    public interface IAuthApiService
    {
        // Sends login credentials to the backend API and returns a LoginResponse
        // containing the JWT token and user info, or null if login failed.
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        // Tells the backend API to invalidate the given JWT token (server-side logout).
        // Returns true if the API accepted the logout request.
        Task<bool> LogoutAsync(string token, CancellationToken cancellation = default);

        // Asks the backend API to change the current user's password.
        // Returns a tuple: Success = true when changed, or an Error message explaining why it failed.
        Task<(bool Success, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    }
}
