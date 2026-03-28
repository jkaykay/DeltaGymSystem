using System.Net.Http.Headers;
using System.Net.Http.Json;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    //this creates the service class and implement the interface( a promise that tells us it must have a login method)
    public class AuthApiService : IAuthApiService
    {
        //create httpclient, this is a tool c# uses to make web requests
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

        public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request, string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsJsonAsync("api/auth/profile", request, cancellationToken);

            return response.IsSuccessStatusCode;
        }
    }
}