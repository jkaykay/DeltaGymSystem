using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Shared.DTOs;
using System.Runtime.InteropServices;

namespace GymSystem.Web.Services
{
    public class MemberApiService : IMemberApiService
    {
        private readonly HttpClient _http;

        public MemberApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("GymApi");
        }

        public async Task<LoginResponse?> MemberLoginAsync(string emailOrUserName, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { emailOrUserName, password });

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task MemberLogoutAsync()
        {
            await _http.PostAsync("api/auth/logout", null);
        }

        public async Task<QRCodeResponse?> GetMyQRAsync(string id)
        {
            var code = await _http.GetAsync($"api/QRCode/generate/{id}");
            if (!code.IsSuccessStatusCode) return null;

            return await code.Content.ReadFromJsonAsync<QRCodeResponse>();
        }
    }
}
