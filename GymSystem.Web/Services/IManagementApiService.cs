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

        // Trainers
        Task<PagedResult<UserDTO>> GetTrainersAsync(int page = 1, int pageSize = 10);
        Task<UserDTO?> GetTrainerAsync(string id);
        Task<bool> CreateTrainerAsync(CreateTrainerViewModel model);
        Task<bool> UpdateTrainerAsync(string id, EditTrainerViewModel model);
        Task<bool> DeleteTrainerAsync(string id);
        Task<CountResponse> GetTotalTrainersAsync();

        // Tiers
        Task<List<TierDTO>> GetTiersAsync();
        Task<TierDTO?> GetTierAsync(string tierName);
        Task<bool> CreateTierAsync(CreateTierViewModel model);
        Task<bool> UpdateTierAsync(string tierName, EditTierViewModel model);
        Task<bool> DeleteTierAsync(string tierName);

        // Branches
        Task<List<BranchDTO>> GetBranchesAsync();
        Task<BranchDTO?> GetBranchAsync(int id);
        Task<bool> CreateBranchAsync(CreateBranchViewModel model);
        Task<bool> UpdateBranchAsync(int id, EditBranchViewModel model);
        Task<bool> DeleteBranchAsync(int id);

        // Rooms
        Task<List<RoomDTO>> GetRoomsAsync();
        Task<RoomDTO?> GetRoomAsync(int id);
        Task<bool> CreateRoomAsync(CreateRoomViewModel model);
        Task<bool> UpdateRoomAsync(int id, EditRoomViewModel model);
        Task<bool> DeleteRoomAsync(int id);
    }
}