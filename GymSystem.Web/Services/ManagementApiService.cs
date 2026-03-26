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
    }
}