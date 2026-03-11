using GymSystem.Web.Areas.Member.ViewModels;
using GymSystem.Web.DTOs.Management;
using GymSystem.Web.DTOs.Member;

namespace GymSystem.Web.Services
{
    public class MemberApiService : IMemberApiService
    {
        private readonly HttpClient _http;

        public MemberApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("GymApi");
        }

        public async Task<MemberLoginResponse?> MemberLoginAsync(string emailOrUserName, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { emailOrUserName, password });

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<MemberLoginResponse>();
        }
    }
}
