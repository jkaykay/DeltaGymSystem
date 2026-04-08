// ============================================================
// MemberController.cs — CRUD endpoints for gym members.
// Admin/Staff can list, create, update, and delete members.
// Members can view and update their own profile.
// ============================================================

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
[Authorize]  // All endpoints require authentication by default
public class MemberController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;  // Manages user accounts
    private readonly GymDbContext _context;                      // Database context
    private readonly IOutputCacheStore _outputCache;             // Invalidates cached responses

    public MemberController(UserManager<ApplicationUser> userManager, GymDbContext context, IOutputCacheStore outputCache)
    {
        _userManager = userManager;
        _context = context;
        _outputCache = outputCache;
    }

    // Helper: builds a query that returns only users in the "Member" role.
    // Uses a join between UserRoles and Roles tables to filter.
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

    // GET api/member — List all members with search, filtering, sorting, and pagination.
    // Only Admin and Staff can access this endpoint.
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [OutputCache(PolicyName = "members")]  // Cache the response for 30 seconds
    public async Task<IActionResult> GetAll([FromQuery] MemberSearchRequest request)
    {
        var query = MembersQuery();

        // --- General search across multiple fields ---
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.Email!.ToLower().Contains(term) ||
                m.UserName!.ToLower().Contains(term));
        }

        // --- Specific filters ---
        if (request.Active.HasValue)
            query = query.Where(m => m.Active == request.Active.Value);

        if (request.JoinedFrom.HasValue)
            query = query.Where(m => m.JoinDate >= request.JoinedFrom.Value);

        if (request.JoinedTo.HasValue)
            query = query.Where(m => m.JoinDate <= request.JoinedTo.Value);

        // --- Sorting ---
        query = ApplySorting(query, request.SortBy, request.SortDir);

        // --- Projection + Pagination ---
        var result = await query
            .Select(m => new UserDTO
            {
                Id = m.Id,
                Email = m.Email!,
                UserName = m.UserName!,
                FirstName = m.FirstName,
                LastName = m.LastName,
                JoinDate = m.JoinDate,
                Active = m.Active,
                PhoneNumber = m.PhoneNumber
            })
            .ToPagedResultAsync(request.Page, request.PageSize);

        return Ok(result);
    }

    // GET api/member/total — Returns the total count of members (for dashboard stats).
    [HttpGet("total")]
    [Authorize(Roles = "Admin,Staff")]
    [OutputCache(PolicyName = "members")]
    public async Task<IActionResult> GetTotal()
    {
        var count = await MembersQuery().CountAsync();
        return Ok(new CountResponse { Count = count });
    }

    // GET api/member/recents — Returns the 5 most recently joined members.
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
                Active = m.Active,
                PhoneNumber = m.PhoneNumber
            })
            .ToListAsync();

        return Ok(result);
    }

    // POST api/member — Admin/Staff creates a new member account.
    // The member starts as inactive until they get a subscription.
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
            PhoneNumber = request.PhoneNumber,
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
            PhoneNumber = user.PhoneNumber,
            Roles = [role]
        });
    }

    // GET api/member/{id} — Get a single member's profile.
    // Admin/Staff can view anyone; members can only view themselves.
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
            Active = user.Active,
            PhoneNumber = user.PhoneNumber
        });
    }

    // PUT api/member/{id} — Update a member's profile.
    // Admin/Staff can update anyone; members can update themselves
    // (but only Admin/Staff can change the Active status).
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
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;

        // Only Admin/Staff can change active status
        if (request.IsActive.HasValue && IsAdminOrStaff())
            user.Active = request.IsActive.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _outputCache.EvictByTagAsync("members", default);

        return NoContent();
    }

    // DELETE api/member/{id} — Permanently delete a member (Admin only).
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

    // Applies dynamic sorting based on query parameters.
    private static IQueryable<ApplicationUser> ApplySorting(
        IQueryable<ApplicationUser> query, string? sortBy, string? sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "firstname" => descending ? query.OrderByDescending(m => m.FirstName) : query.OrderBy(m => m.FirstName),
            "lastname" => descending ? query.OrderByDescending(m => m.LastName) : query.OrderBy(m => m.LastName),
            "email" => descending ? query.OrderByDescending(m => m.Email) : query.OrderBy(m => m.Email),
            "username" => descending ? query.OrderByDescending(m => m.UserName) : query.OrderBy(m => m.UserName),
            "active" => descending ? query.OrderByDescending(m => m.Active) : query.OrderBy(m => m.Active),
            _ => descending ? query.OrderByDescending(m => m.JoinDate) : query.OrderBy(m => m.JoinDate),
        };
    }

    // Checks if the current user is requesting their own data.
    private bool IsSelf(string id) =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) == id;

    // Checks if the current user is an Admin or Staff member.
    private bool IsAdminOrStaff() =>
        User.IsInRole("Admin") || User.IsInRole("Staff");
}