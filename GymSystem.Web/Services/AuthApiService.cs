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
            //this creates the API client
            var client = _httpClientFactory.CreateClient("GymApi");

            //this sends post/login request
            var response = await client.PostAsJsonAsync("api/auth/login", request, cancellationToken);

            //check for success
            if (!response.IsSuccessStatusCode)
                return null;

            //read the API response, turn the JSOn response into a loginresponse object
            return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        }

        //send request to the api and check if updateprofile worked or not
        public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request, string token, CancellationToken cancellationToken = default)
        {
            //create an http client
            var client = _httpClientFactory.CreateClient("GymApi");

            //send this request as currently logged in user
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            //send HTTP PUT request
            var response = await client.PutAsJsonAsync("api/auth/profile", request, cancellationToken);

            //check if success
            return response.IsSuccessStatusCode;
        }
    }
}