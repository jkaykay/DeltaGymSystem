using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MemberController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GymDbContext _context;
    private readonly IOutputCacheStore _outputCache;

    public MemberController(UserManager<ApplicationUser> userManager, GymDbContext context, IOutputCacheStore outputCache)
    {
        _userManager = userManager;
        _context = context;
        _outputCache = outputCache;
    }

    private IQueryable<ApplicationUser> MembersQuery()
    {
        return _context.Users
            .Where(u => _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .Any(x => x.UserId == u.Id && x.Name == "Member"));
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [OutputCache(PolicyName = "members")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await MembersQuery()
            .Select(m => new UserDTO
            {
                Id = m.Id,
                Email = m.Email!,
                UserName = m.UserName!,
                FirstName = m.FirstName,
                LastName = m.LastName,
                JoinDate = m.JoinDate,
                Active = m.Active
            })
            .ToPagedResultAsync(page, pageSize);

        return Ok(result);
    }

    [HttpGet("total")]
    [Authorize(Roles = "Admin,Staff")]
    [OutputCache(PolicyName = "members")]
    public async Task<IActionResult> GetTotal()
    {
        var count = await MembersQuery().CountAsync();
        return Ok(new CountResponse { Count = count });
    }

    [HttpGet("recents")]
    [Authorize(Roles = "Admin,Staff")]
    [OutputCache(PolicyName = "members")]
    public async Task<IActionResult> GetRecentSignups()
    {
        var result = await MembersQuery()
            .OrderByDescending(m => m.JoinDate)
            .Take(5)
            .Select(m => new UserDTO
            {
                Id = m.Id,
                Email = m.Email!,
                UserName = m.UserName!,
                FirstName = m.FirstName,
                LastName = m.LastName,
                JoinDate = m.JoinDate,
                Active = m.Active
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
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
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var role = "Member";
        await _userManager.AddToRoleAsync(user, role);

        await _outputCache.EvictByTagAsync("members", default);

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

        if (request.Email is not null)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null && existing.Id != user.Id)
                return Conflict("A user with this email already exists.");

            user.Email = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
        }

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;

        // Only Admin/Staff can change active status
        if (request.IsActive.HasValue && IsAdminOrStaff())
            user.Active = request.IsActive.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _outputCache.EvictByTagAsync("members", default);

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

        await _outputCache.EvictByTagAsync("members", default);

        return NoContent();
    }

    private bool IsSelf(string id) =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) == id;

    private bool IsAdminOrStaff() =>
        User.IsInRole("Admin") || User.IsInRole("Staff");
}