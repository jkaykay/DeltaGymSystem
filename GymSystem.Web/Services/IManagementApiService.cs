using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public interface IManagementApiService
    {
        // Auth
        Task<LoginResponse?> LoginAsync(string email, string password);
        Task LogoutAsync();

        // Members
        Task<bool> CreateMemberAsync(CreateMemberViewModel model);
        Task<PagedResult<UserDTO>> GetMembersAsync(int page = 1, int pageSize = 10);
        Task<UserDTO?> GetMemberAsync(string id);
        Task<bool> UpdateMemberAsync(string id, EditMemberViewModel model);
        Task<bool> ToggleMemberActiveAsync(string id);
        Task<bool> DeleteMemberAsync(string id);
        Task<CountResponse> GetTotalMembersAsync();
        Task<List<UserDTO>> GetRecentSignupsAsync();

        // Staff
        Task<PagedResult<UserDTO>> GetStaffAsync(int page = 1, int pageSize = 10);
        Task<UserDTO?> GetStaffMemberAsync(string id);
        Task<bool> CreateStaffAsync(CreateStaffViewModel model);
        Task<bool> UpdateStaffAsync(string id, EditStaffViewModel model);
        Task<bool> DeleteStaffAsync(string id);
        Task<CountResponse> GetTotalStaffAsync();

        //Trainer
        Task<CountResponse> GetTotalTrainersAsync();
    }
}