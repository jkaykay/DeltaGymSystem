using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.DTOs.Management;
using System.Net.Http.Json;

namespace GymSystem.Web.Services;

public interface IManagementApiService
{
    // Auth
    Task<ManagementLoginResponse?> LoginAsync(string email, string password);

    // Members
    Task<bool> CreateMemberAsync(CreateMemberViewModel model);
    Task<List<MemberResponse>> GetMembersAsync();
    Task<MemberResponse?> GetMemberAsync(string id);
    Task<bool> UpdateMemberAsync(string id, EditMemberViewModel model);
    Task<bool> ToggleMemberActiveAsync(string id);
    Task<bool> DeleteMemberAsync(string id);
    Task<CountResponse> GetTotalMembersAsync();
    Task<List<MemberResponse>> GetRecentSignupsAsync();

    // Staff
    Task<List<StaffResponse>> GetStaffAsync();
    Task<StaffResponse?> GetStaffMemberAsync(string id);
    Task<bool> CreateStaffAsync(CreateStaffViewModel model);
    Task<bool> UpdateStaffAsync(string id, EditStaffViewModel model);
    Task<bool> DeleteStaffAsync(string id);
    Task<CountResponse> GetTotalStaffAsync();
    
}