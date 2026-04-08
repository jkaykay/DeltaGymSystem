using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    /// <summary>
    /// Interface for the authentication API service.
    /// Defines the contract for login, logout, and password-change operations.
    /// Any class that implements this interface must provide these methods.
    /// </summary>
    public interface IAuthApiService
    {
        /// <summary>
        /// Sends login credentials to the backend API and returns a LoginResponse
        /// containing the JWT token and user info, or null if login failed.
        /// </summary>
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tells the backend API to invalidate the given JWT token (server-side logout).
        /// Returns true if the API accepted the logout request.
        /// </summary>
        Task<bool> LogoutAsync(string token, CancellationToken cancellation = default);

        /// <summary>
        /// Asks the backend API to change the current user's password.
        /// Returns a tuple: Success = true when changed, or an Error message explaining why it failed.
        /// </summary>
        Task<(bool Success, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    }
}