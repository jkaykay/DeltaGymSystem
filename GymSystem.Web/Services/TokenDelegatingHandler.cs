using Microsoft.AspNetCore.Authentication;

namespace GymSystem.Web.Services
{
    public class TokenDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                var token = await httpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new("Bearer", token);
                }
            }

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