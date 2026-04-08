using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    // Implements IAuthApiService.
    // This service talks to the backend REST API to handle authentication tasks:
    // logging in, logging out, and changing passwords.
    // It uses IHttpClientFactory to create HTTP clients that call the API.
    public class AuthApiService : IAuthApiService
    {
        // IHttpClientFactory lets us create HttpClient instances on demand.
        // We use a named client ("GymApi") so the base URL and token handler are preconfigured.
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor — called by the DI container. The factory is injected automatically.
        public AuthApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Sends the user's email and password to POST api/auth/login.
        // If the credentials are valid the API returns a LoginResponse (JWT token + user info).
        // Returns null when the credentials are wrong (non-success status code).
        public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            // Create an HttpClient with the preconfigured base URL.
            var client = _httpClientFactory.CreateClient("GymApi");

            // POST the login request as JSON to the API.
            var response = await client.PostAsJsonAsync("api/auth/login", request, cancellationToken);

            // If the API returned an error status (e.g. 401 Unauthorized), return null.
            if (!response.IsSuccessStatusCode)
                return null;

            // Deserialise the JSON response body into a LoginResponse object.
            return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        }


        // Tells the API to invalidate the user's JWT token (server-side sign-out).
        // The Bearer token is sent in the Authorization header so the API knows which session to end.
        // Returns true when the API accepted the request.
        public async Task<bool> LogoutAsync(string token, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            // Attach the JWT token so the API can identify and invalidate it.
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("api/auth/logout", null, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        // Asks the API to change the authenticated user's password.
        // Returns (true, null) on success, or (false, errorMessage) on failure.
        public async Task<(bool Success, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("GymApi");

            var request = new ChangePasswordRequest(currentPassword, newPassword);
            var response = await client.PostAsJsonAsync("api/auth/change-password", request, cancellationToken);

            if (response.IsSuccessStatusCode)
                return (true, null);

            // Read the error body and try to extract a user-friendly message.
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return (false, ExtractChangePasswordError(error));
        }

        // Helper: tries to parse the API error as an array of Identity error objects
        // and join their descriptions into a single readable string.
        // Falls back to the raw error text when parsing fails.
        private static string ExtractChangePasswordError(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return "Password change failed.";

            try
            {
                var identityErrors = JsonSerializer.Deserialize<List<IdentityErrorResponse>>(error);
                var descriptions = identityErrors?
                    .Select(e => e.Description)
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();

                if (descriptions is { Count: > 0 })
                    return string.Join(" ", descriptions);
            }
            catch (JsonException)
            {
                // Fall back to the raw API response when it isn't an Identity error array.
            }

            return error;
        }

        // Small helper class that mirrors the shape of ASP.NET Identity error objects
        // returned by the API (e.g. { "description": "Password too short" }).
        private sealed class IdentityErrorResponse
        {
            public string Description { get; set; } = string.Empty;
        }
    }
}
