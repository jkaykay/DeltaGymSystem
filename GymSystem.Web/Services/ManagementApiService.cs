using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public class ManagementApiService : IManagementApiService
    {
        private readonly HttpClient _http;

        public ManagementApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("GymApi");
        }

        // --- Auth ---

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { EmailOrUserName = email, password });

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        // --- Members ---

        public async Task<PagedResult<UserDTO>> GetMembersAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                $"api/member?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<UserDTO>();
        }

        public async Task<UserDTO?> GetMemberAsync(string id)
        {
            var response = await _http.GetAsync($"api/member/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/member", new
            {
                model.Email,
                UserName = model.Username,
                model.FirstName,
                model.LastName,
                model.Password
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMemberAsync(string id, EditMemberViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/member/{id}", new
            {
                model.FirstName,
                model.LastName,
                model.Active
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleMemberActiveAsync(string id)
        {
            var response = await _http.PostAsync($"api/member/{id}/toggle-active", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMemberAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/member/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- Staff ---

        public async Task<PagedResult<UserDTO>> GetStaffAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                $"api/staff?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<UserDTO>();
        }

        public async Task<UserDTO?> GetStaffMemberAsync(string id)
        {
            var response = await _http.GetAsync($"api/staff/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<bool> CreateStaffAsync(CreateStaffViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/staff", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.Password,
                model.EmployeeId,
                model.Role
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateStaffAsync(string id, EditStaffViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/staff/{id}", new
            {
                model.Email,
                model.FirstName,
                model.LastName
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteStaffAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/staff/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<CountResponse> GetTotalMembersAsync()
        {
            var response = await _http.GetAsync("api/member/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        public async Task<CountResponse> GetTotalStaffAsync()
        {
            var response = await _http.GetAsync("api/staff/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        public async Task<List<UserDTO>> GetRecentSignupsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<UserDTO>>("api/member/recents");
            return result ?? new List<UserDTO>();
        }

        public async Task LogoutAsync()
        {
            await _http.PostAsync("api/auth/logout", null);
        }

        // --- Branches ---

        public async Task<List<BranchDTO>> GetBranchesAsync()
        {
            var result = await _http.GetFromJsonAsync<List<BranchDTO>>("api/branch");
            return result ?? new List<BranchDTO>();
        }

        public async Task<BranchDTO?> GetBranchAsync(int id)
        {
            var response = await _http.GetAsync($"api/branch/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<BranchDTO>();
        }

        public async Task<bool> CreateBranchAsync(CreateBranchViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/branch", new
            {
                model.Address,
                model.City,
                model.Province,
                model.PostCode,
                model.OpenDate
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateBranchAsync(int id, EditBranchViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/branch/{id}", new
            {
                model.Address,
                model.City,
                model.Province,
                model.PostCode
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBranchAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/branch/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- Tiers ---

        public async Task<List<TierDTO>> GetTiersAsync()
        {
            var result = await _http.GetFromJsonAsync<List<TierDTO>>("api/tier");
            return result ?? new List<TierDTO>();
        }

        public async Task<TierDTO?> GetTierAsync(string tierName)
        {
            var response = await _http.GetAsync($"api/tier/{tierName}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<TierDTO>();
        }

        public async Task<bool> CreateTierAsync(CreateTierViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/tier", new
            {
                model.TierName,
                model.Price
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTierAsync(string tierName, EditTierViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/tier/{tierName}", new
            {
                model.Price
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTierAsync(string tierName)
        {
            var response = await _http.DeleteAsync($"api/tier/{tierName}");
            return response.IsSuccessStatusCode;
        }

        // --- Trainers ---
        public async Task<CountResponse> GetTotalTrainersAsync()
        {
            var response = await _http.GetAsync("api/trainer/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        public async Task<PagedResult<UserDTO>> GetTrainersAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                $"api/trainer?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<UserDTO>();
        }

        public async Task<UserDTO?> GetTrainerAsync(string id)
        {
            var response = await _http.GetAsync($"api/trainer/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<bool> CreateTrainerAsync(CreateTrainerViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/trainer", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.Password,
                model.EmployeeId,
                model.BranchId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTrainerAsync(string id, EditTrainerViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/trainer/{id}", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.EmployeeId,
                model.BranchId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTrainerAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/trainer/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- Rooms ---

        public async Task<List<RoomDTO>> GetRoomsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<RoomDTO>>("api/room");
            return result ?? new List<RoomDTO>();
        }

        public async Task<RoomDTO?> GetRoomAsync(int id)
        {
            var response = await _http.GetAsync($"api/room/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<RoomDTO>();
        }

        public async Task<bool> CreateRoomAsync(CreateRoomViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/room", new
            {
                model.RoomNumber,
                model.BranchId,
                model.MaxCapacity
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRoomAsync(int id, EditRoomViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/room/{id}", new
            {
                model.RoomNumber,
                model.BranchId,
                model.MaxCapacity
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/room/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}