using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services;

public interface IManagementApiService
{
    // Auth
    Task<LoginResponse?> LoginAsync(string email, string password);

    // Members
    Task<bool> CreateMemberAsync(CreateMemberViewModel model);
    Task<List<UserDto>> GetMembersAsync();
    Task<UserDto?> GetMemberAsync(string id);
    Task<bool> UpdateMemberAsync(string id, EditMemberViewModel model);
    Task<bool> ToggleMemberActiveAsync(string id);
    Task<bool> DeleteMemberAsync(string id);
    Task<CountResponse> GetTotalMembersAsync();
    Task<List<UserDto>> GetRecentSignupsAsync();

    // Staff
    Task<List<UserDto>> GetStaffAsync();
    Task<UserDto?> GetStaffMemberAsync(string id);
    Task<bool> CreateStaffAsync(CreateStaffViewModel model);
    Task<bool> UpdateStaffAsync(string id, EditStaffViewModel model);
    Task<bool> DeleteStaffAsync(string id);
    Task<CountResponse> GetTotalStaffAsync();

}