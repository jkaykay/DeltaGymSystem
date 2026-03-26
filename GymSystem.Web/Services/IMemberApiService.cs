using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public interface IMemberApiService
    {
        Task<LoginResponse?> MemberLoginAsync(string emailOrUserName, string password);
        Task MemberLogoutAsync();
    }
}
