using GymSystem.Api.DTOs;
using GymSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll()
    {
        var staffUsers = await _userManager.GetUsersInRoleAsync("Staff");
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

        var allStaff = staffUsers
            .Union(adminUsers, new UserIdComparer())
            .ToList();

        var result = new List<UserDto>();
        foreach (var s in allStaff)
        {
            var roles = await _userManager.GetRolesAsync(s);
            result.Add(new UserDto
            {
                Id = s.Id,
                Email = s.Email!,
                UserName = s.UserName!,
                FirstName = s.FirstName,
                LastName = s.LastName,
                HireDate = s.HireDate,
                EmployeeId = s.EmployeeId,
                Active = s.Active,
                Roles = [.. roles]
            });
        }

        return Ok(result);
    }

    [HttpGet("total")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetTotal()
    {
        var staff = await _userManager.GetUsersInRoleAsync("Staff");
        var admin = await _userManager.GetUsersInRoleAsync("Admin");
        var total = staff.Count + admin.Count;
        return Ok(new { Count = total });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Get(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Staff") && !roles.Contains("Admin"))
            return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            HireDate = user.HireDate,
            EmployeeId = user.EmployeeId,
            Active = user.Active,
            Roles = [.. roles]
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request)
    {
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

        var role = request.Role is "Admin" or "Staff" ? request.Role : "Staff";
        await _userManager.AddToRoleAsync(user, role);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, new { user.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateStaffRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

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

    private sealed class UserIdComparer : IEqualityComparer<ApplicationUser>
    {
        public bool Equals(ApplicationUser? x, ApplicationUser? y) => x?.Id == y?.Id;
        public int GetHashCode(ApplicationUser obj) => obj.Id.GetHashCode();
    }
}