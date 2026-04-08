using Microsoft.AspNetCore.Authentication;

namespace GymSystem.Web.Services
{
    // A custom HTTP message handler that automatically attaches the user's JWT
    // (access_token) to every outgoing API request.
    // How it works:
    //  1. The user logs in and the JWT is stored inside the authentication cookie.
    //  2. When any service makes an HTTP call via the named "GymApi" client,
    //     this handler intercepts the request before it leaves the app.
    //  3. It reads the JWT from the cookie and adds an "Authorization: Bearer {token}"
    //     header so the backend API can authenticate the call.
    //  4. If the API responds with 401 Unauthorized (e.g. token expired), it
    //     automatically signs the user out so they are redirected to the login page.
    public class TokenDelegatingHandler : DelegatingHandler
    {
        // Provides access to the current HTTP request/response context.
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Intercepts every outgoing HTTP request, attaches the JWT token,
        // sends the request, and handles 401 responses.
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Try to read the JWT from the user's authentication cookie.
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                var token = await httpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(token))
                {
                    // Add the token as a Bearer header so the API recognises the user.
                    request.Headers.Authorization = new("Bearer", token);
                }
            }

            // Let the request continue to the API and wait for the response.
            var response = await base.SendAsync(request, cancellationToken);

            // If the API rejects the token, sign the user out so the cookie
            // middleware redirects them to the appropriate login page.
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                && httpContext?.User.Identity?.IsAuthenticated == true)
            {
                await httpContext.SignOutAsync("Cookies");
            }

            return response;
        }
    }
}
