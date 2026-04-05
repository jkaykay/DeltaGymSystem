using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public interface IAuthApiService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        Task<bool> LogoutAsync(string token, CancellationToken cancellation = default);
        

    }
}