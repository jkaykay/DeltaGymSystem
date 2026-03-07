using GymSystem.Api.DTOs;
using GymSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Staff")]
public class MembersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public MembersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var members = await _userManager.GetUsersInRoleAsync("Member");
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
        var members = await _userManager.GetUsersInRoleAsync("Member");
        return Ok(new { TotalMembers = members.Count });
    }

    [HttpGet("recents")]
    public async Task<IActionResult> GetRecentSignups()
    {
        var members = await _userManager.GetUsersInRoleAsync("Member");
        var recentMembers = members.OrderByDescending(m => m.JoinDate).Take(5).ToList();
        var result = recentMembers.Select(m => new UserDto
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

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Member"))
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMemberRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Active = request.Active;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

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

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}