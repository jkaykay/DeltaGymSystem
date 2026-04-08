// ============================================================
// AuthController.cs — Handles authentication endpoints:
// register, login, logout, get current user, change password.
// These are the first endpoints a client interacts with.
// ============================================================

using GymSystem.Api.Models;
using GymSystem.Api.Services;
using GymSystem.Api.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using GymSystem.Shared.DTOs;
using Microsoft.EntityFrameworkCore;


namespace GymSystem.Api.Controllers
{
    // [Route] sets the base URL: all endpoints start with "api/auth".
    // [ApiController] enables automatic model validation and JSON binding.
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;   // ASP.NET Identity: manages users
        private readonly ITokenService _tokenService;                 // Generates JWT tokens
        private readonly ITokenRevocationService _revocationService;  // Blacklists tokens on logout
        private readonly GymDbContext _context;                       // Database context

        // Constructor — dependencies are injected by the DI container.
        public AuthController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            ITokenRevocationService revocationService,
            GymDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _revocationService = revocationService;
            _context = context;
        }

        // POST api/auth/register — Create a new member account.
        // Rate-limited to prevent abuse. New members start as inactive
        // until they purchase a subscription.
        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingByEmail is not null)
                return Conflict("A user with this email already exists.");

            var existingByUsername = await _userManager.FindByNameAsync(request.UserName);
            if (existingByUsername is not null)
                return Conflict("A user with this username already exists.");

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                JoinDate = DateTime.UtcNow,
                Active = false
            };
            
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Member");

            return Ok(new { message = "Registration successful." });
        }

        // POST api/auth/login — Authenticate a user and return a JWT token.
        // The client can log in with either email or username.
        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Try to find the user by email first, then by username.
            var user = await _userManager.FindByEmailAsync(request.EmailOrUserName)
                       ?? await _userManager.FindByNameAsync(request.EmailOrUserName);


            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new { message = "Invalid email/username or password." });

            string gymLocation = "";

            if (user.BranchId != null) 
            {

                var branch = await _context.Branches.FindAsync(user.BranchId);

                if (branch != null)
                {

                    gymLocation = $"{branch.City}, {branch.Province}";
                }
            }
              

            var token = await _tokenService.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new LoginResponse(
                token,
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                gymLocation,
                [.. roles]
            ));
        }

        // GET api/auth/me — Return the currently logged-in user's profile.
        // [Authorize] means only authenticated users can call this.
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            // Extract the user's ID from the JWT token claims.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UserDTO
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JoinDate = user.JoinDate,
                HireDate = user.HireDate,
                EmployeeId = user.EmployeeId,
                BranchId = user.BranchId,
                Active = user.Active,
                PhoneNumber = user.PhoneNumber,
                Roles = [.. roles]
            });
        }

        // POST api/auth/change-password — Change the current user's password.
        // Requires the old password for verification.
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user is null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        // POST api/auth/logout — Invalidate the current JWT token.
        // Adds the token's unique ID (JTI) to the blacklist so it
        // cannot be used again, even if it hasn't expired yet.
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Read the token's unique ID and expiry from the claims.
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

            if (jti is null || expClaim is null)
                return BadRequest(new { message = "Token is missing required claims." });

            var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
            _revocationService.Revoke(jti, expiry);

            return NoContent();
        }


   
    }


            


        
}