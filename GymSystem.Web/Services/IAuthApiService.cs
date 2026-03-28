using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public interface IAuthApiService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        Task<bool> UpdateProfileAsync(UpdateProfileRequest request, string token, CancellationToken cancellationToken = default);
    }
}