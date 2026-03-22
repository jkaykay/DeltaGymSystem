using GymSystem.Shared.DTOs;
using GymSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MemberController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public MemberController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll()
    {
        var members = await _userManager.GetUsersInRoleAsync("Member");
        var result = members.Select(m => new UserDTO
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
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetTotal()
    {
        var members = await _userManager.GetUsersInRoleAsync("Member");
        return Ok(new CountResponse { Count = members.Count });
    }

    [HttpGet("recents")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetRecentSignups()
    {
        var members = await _userManager.GetUsersInRoleAsync("Member");
        var recentMembers = members.OrderByDescending(m => m.JoinDate).Take(5).ToList();
        var result = recentMembers.Select(m => new UserDTO
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

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
            return Conflict("A user with this email already exists.");

        var existingByUsername = await _userManager.FindByNameAsync(request.Username);
        if (existingByUsername is not null)
            return Conflict("A user with this username already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            JoinDate = DateTime.UtcNow,
            Active = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var role = "Member";
        await _userManager.AddToRoleAsync(user, role);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            JoinDate = user.JoinDate,
            Active = user.Active,
            Roles = [role]
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        if (!IsAdminOrStaff() && !IsSelf(id))
            return Forbid();

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Member"))
            return NotFound();

        return Ok(new UserDTO
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMemberRequest request)
    {
        if (!IsAdminOrStaff() && !IsSelf(id))
            return Forbid();

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Member"))
            return NotFound("User is not a member.");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;

        // Only Admin/Staff can change active status
        if (request.Active.HasValue && IsAdminOrStaff())
            user.Active = request.Active.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{id}/toggle-active")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Member"))
            return NotFound("User is not a member.");

        user.Active = !user.Active;
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

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Member"))
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    private bool IsSelf(string id) =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) == id;

    private bool IsAdminOrStaff() =>
        User.IsInRole("Admin") || User.IsInRole("Staff");
}