using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.DTOs.Member;

namespace GymSystem.Web.Services
{
    public interface IMemberApiService
    {
        Task<MemberLoginResponse?> MemberLoginAsync(string emailOrUserName, string password);
    }
}
