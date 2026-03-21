using GymSystem.Shared.DTOs;
using GymSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class TrainerController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainerController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var members = await _userManager.GetUsersInRoleAsync("Trainer");
            var result = members.Select(m => new UserDto
            {
                Id = m.Id,
                Email = m.Email!,
                UserName = m.UserName!,
                FirstName = m.FirstName,
                LastName = m.LastName,
                JoinDate = m.JoinDate,
                Active = m.Active
            }).ToList();

            return Ok(result);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotal()
        {
            var members = await _userManager.GetUsersInRoleAsync("Trainer");
            return Ok(new CountResponse { Count = members.Count });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Trainer"))
                return NotFound();

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JoinDate = user.JoinDate,
                Active = user.Active
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTrainerRequest request)
        {
            var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingByEmail is not null)
                return Conflict("A user with this email already exists.");

            var username = !string.IsNullOrWhiteSpace(request.EmployeeId)
                    ? request.EmployeeId.Replace("-", "").ToLowerInvariant()
                    : request.Email.Split('@')[0];

            var user = new ApplicationUser
            {
                UserName = username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmployeeId = request.EmployeeId,
                HireDate = DateTime.UtcNow,
                Active = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var role = "Trainer"; 
            await _userManager.AddToRoleAsync(user, role);

            return CreatedAtAction(nameof(Get), new { id = user.Id }, new UserDto 
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmployeeId = user.EmployeeId,
                HireDate = user.HireDate,
                Active = user.Active
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateTrainerRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Trainer"))
                return NotFound();

            // Check for duplicate email (excluding current user)
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existing = await _userManager.FindByEmailAsync(request.Email);
                if (existing is not null && existing.Id != user.Id)
                    return Conflict("A user with this email already exists.");
            }

            user.Email = request.Email ?? user.Email;

            if(request.Email != null)
            {
                var username = !string.IsNullOrWhiteSpace(request.Email) ? request.Email.Split('@')[0] : null;
                user.UserName = username; // Update username if email changes
            }

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.EmployeeId = request.EmployeeId ?? user.EmployeeId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }
    }
}