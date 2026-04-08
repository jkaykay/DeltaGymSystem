// ============================================================
// TokenService.cs — Generates JWT (JSON Web Token) for user login.
// When a user logs in, this service creates a signed token containing
// the user's identity (ID, email, name) and roles. The client sends
// this token in the "Authorization" header with every API request.
// ============================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymSystem.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace GymSystem.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager; // Looks up user roles
        private readonly IConfiguration _config;                    // Reads JWT settings from appsettings.json

        // Dependencies are injected automatically by the DI container.
        public TokenService(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        // Builds a JWT token containing the user's claims (identity info + roles).
        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            // Look up all roles assigned to this user (e.g. "Admin", "Member")
            var roles = await _userManager.GetRolesAsync(user);

            // Claims are key-value pairs embedded inside the token.
            // The server reads these to identify the user on each request.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID (for revocation)
                new(ClaimTypes.NameIdentifier, user.Id),                     // User's database ID
                new(ClaimTypes.Email, user.Email!),                          // Email address
                new(ClaimTypes.GivenName, user.FirstName),                   // First name
                new(ClaimTypes.Surname, user.LastName)                       // Last name
            };

            // Add a role claim for each role so [Authorize(Roles = "...")] works.
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create the signing key from the secret stored in configuration.
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            // HMAC-SHA256 is the algorithm used to sign (and later verify) the token.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token expiry — defaults to 15 days if not configured.
            var expiryDays = int.Parse(_config["Jwt:ExpiryDays"] ?? "15");

            // Assemble the token with all its parts.
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryDays),
                signingCredentials: creds);

            // Serialize the token to a compact string (e.g. "eyJhbGci...")
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}