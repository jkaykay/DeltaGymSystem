using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.DTOs.Management;
using System.Net.Http.Json;

namespace GymSystem.Web.Services;

public interface IManagementApiService
{
    // Auth
    Task<ManagementLoginResponse?> LoginAsync(string email, string password);

    // Members
    Task<List<MemberResponse>> GetMembersAsync();
    Task<MemberResponse?> GetMemberAsync(string id);
    Task<bool> UpdateMemberAsync(string id, EditMemberViewModel model);
    Task<bool> ToggleMemberActiveAsync(string id);
    Task<bool> DeleteMemberAsync(string id);

    // Staff
    Task<List<StaffResponse>> GetStaffAsync();
    Task<StaffResponse?> GetStaffMemberAsync(string id);
    Task<bool> CreateStaffAsync(CreateStaffViewModel model);
    Task<bool> UpdateStaffAsync(string id, EditStaffViewModel model);
    Task<bool> DeleteStaffAsync(string id);
    Task<CountResponse> GetTotalMembersAsync();
    Task<CountResponse> GetTotalStaffAsync();
    Task<List<MemberResponse>> GetRecentSignupsAsync();
}