using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Web.DTOs.Management;

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

        public async Task<ManagementLoginResponse?> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { EmailOrUserName = email, password });

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ManagementLoginResponse>();
        }

        // --- Members ---

        public async Task<List<MemberResponse>> GetMembersAsync()
        {
            var result = await _http.GetFromJsonAsync<List<MemberResponse>>("api/member");
            return result ?? [];
        }

        public async Task<MemberResponse?> GetMemberAsync(string id)
        {
            var response = await _http.GetAsync($"api/member/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<MemberResponse>();
        }

        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model) {
            var response = await _http.PostAsJsonAsync("api/member", new
            {
                model.Email,
                model.Username,
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

        public async Task<List<StaffResponse>> GetStaffAsync()
        {
            var result = await _http.GetFromJsonAsync<List<StaffResponse>>("api/staff");
            return result ?? [];
        }

        public async Task<StaffResponse?> GetStaffMemberAsync(string id)
        {
            var response = await _http.GetAsync($"api/staff/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<StaffResponse>();
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

        public async Task<List<MemberResponse>> GetRecentSignupsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<MemberResponse>>("api/member/recents");
            return result ?? new List<MemberResponse>();
        }
    }
}