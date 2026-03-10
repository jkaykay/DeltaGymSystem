using GymSystem.Api.DTOs;
using GymSystem.Api.Models;
using GymSystem.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                JoinDate = DateTime.UtcNow,
                Active = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Member");

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Try email first, then fall back to username
            var user = await _userManager.FindByEmailAsync(request.EmailOrUserName)
                       ?? await _userManager.FindByNameAsync(request.EmailOrUserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new { message = "Invalid email/username or password." });

            var token = await _tokenService.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new LoginResponse(
                token,
                user.Id,
                user.UserName,
                user.Email!,
                user.FirstName,
                user.LastName,
                [.. roles]
            ));
        }
    }
}