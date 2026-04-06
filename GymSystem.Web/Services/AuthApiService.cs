using System.Net.Http.Headers;
using System.Net.Http.Json;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public class AuthApiService : IAuthApiService
    {
        //create httpclient
        private readonly IHttpClientFactory _httpClientFactory;

        //constructor 
        public AuthApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        //main method to send this login data to the API and return what came back
        public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {

            var client = _httpClientFactory.CreateClient("GymApi");


            var response = await client.PostAsJsonAsync("api/auth/login", request, cancellationToken);


            if (!response.IsSuccessStatusCode)
                return null;


            return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        }


        public async Task<bool> LogoutAsync(string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("api/auth/logout", null, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            var request = new ChangePasswordRequest(currentPassword, newPassword);
            var response = await client.PostAsJsonAsync("api/auth/change-password", request, cancellationToken);

            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, error);
        }
    }
}