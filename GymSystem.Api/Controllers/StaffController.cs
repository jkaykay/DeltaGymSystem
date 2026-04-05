using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Staff")]
public class StaffController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GymDbContext _context;
    private readonly IOutputCacheStore _outputCache;

    public StaffController(UserManager<ApplicationUser> userManager, GymDbContext context, IOutputCacheStore outputCache)
    {
        _userManager = userManager;
        _context = context;
        _outputCache = outputCache;
    }

    private IQueryable<ApplicationUser> StaffQuery()
    {
        return _context.Users
            .Where(u => _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .Any(x => x.UserId == u.Id && (x.Name == "Staff" || x.Name == "Admin")));
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [OutputCache(PolicyName = "staff")]
    public async Task<IActionResult> GetAll([FromQuery] StaffSearchRequest request)
    {
        var query = StaffQuery();

        // --- General search across multiple fields ---
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.Email!.ToLower().Contains(term) ||
                u.UserName!.ToLower().Contains(term) ||
                (u.EmployeeId != null && u.EmployeeId.ToLower().Contains(term)));
        }

        // --- Specific filters ---
        if (request.HiredFrom.HasValue)
            query = query.Where(u => u.HireDate >= request.HiredFrom.Value);

        if (request.HiredTo.HasValue)
            query = query.Where(u => u.HireDate <= request.HiredTo.Value);

        // --- Sorting ---
        query = ApplySorting(query, request.SortBy, request.SortDir);

        // --- Projection + Pagination ---
        var pagedStaff = await query
            .Select(u => new
            {
                u.Id,
                Email = u.Email!,
                UserName = u.UserName!,
                u.FirstName,
                u.LastName,
                u.HireDate,
                u.EmployeeId,
                u.Active,
                u.BranchId,
                Roles = _context.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.Name!)
                    .ToList()
            })
            .ToPagedResultAsync(request.Page, request.PageSize);

        var result = new PagedResult<UserDTO>
        {
            Page = pagedStaff.Page,
            PageSize = pagedStaff.PageSize,
            TotalCount = pagedStaff.TotalCount,
            Items = pagedStaff.Items.Select(s => new UserDTO
            {
                Id = s.Id,
                Email = s.Email,
                UserName = s.UserName,
                FirstName = s.FirstName,
                LastName = s.LastName,
                HireDate = s.HireDate,
                EmployeeId = s.EmployeeId,
                Active = s.Active,
                BranchId = s.BranchId,
                Roles = s.Roles
            }).ToList()
        };

        return Ok(result);
    }

    [HttpGet("total")]
    [Authorize(Roles = "Admin,Staff")]
    [OutputCache(PolicyName = "staff")]
    public async Task<IActionResult> GetTotal()
    {
        var count = await StaffQuery().CountAsync();
        return Ok(new CountResponse { Count = count });
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

        return Ok(new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            HireDate = user.HireDate,
            EmployeeId = user.EmployeeId,
            Active = user.Active,
            BranchId = user.BranchId,
            Roles = [.. roles]
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request)
    {
        if (request.Role is not "Admin" and not "Staff")
            return BadRequest($"Invalid role '{request.Role}'. Must be 'Admin' or 'Staff'.");

        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
            return Conflict("A user with this email already exists.");

        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
        {
            var duplicateEmployeeId = await StaffQuery()
                .AnyAsync(u => u.EmployeeId == request.EmployeeId);

            if (duplicateEmployeeId)
                return Conflict("A staff member with this Employee ID already exists.");
        }

        if (request.BranchId.HasValue)
        {
            var branchExists = await _context.Branches.FindAsync(request.BranchId.Value) is not null;
            if (!branchExists)
                return BadRequest($"Branch with ID {request.BranchId} does not exist.");
        }

        var username = !string.IsNullOrWhiteSpace(request.EmployeeId)
                ? request.EmployeeId.Replace("-", "").ToLowerInvariant()
                : request.Email.Split('@')[0];

        var existingByUsername = await _userManager.FindByNameAsync(username);
        if (existingByUsername is not null)
            return Conflict("A user with the derived username already exists.");

        var user = new ApplicationUser
        {
            UserName = username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmployeeId = request.EmployeeId,
            HireDate = DateTime.UtcNow,
            Active = true,
            BranchId = request.BranchId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, request.Role);

        await _outputCache.EvictByTagAsync("staff", default);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            HireDate = user.HireDate,
            EmployeeId = user.EmployeeId,
            Active = user.Active,
            BranchId = user.BranchId,
            Roles = [request.Role]
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateStaffRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Staff") && !roles.Contains("Admin"))
            return NotFound("User is not a staff member.");

        if (request.Email is not null)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null && existing.Id != user.Id)
                return Conflict("A user with this email already exists.");

            user.Email = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(user.EmployeeId))
            {
                user.UserName = request.Email.Split('@')[0];
                user.NormalizedUserName = user.UserName.ToUpperInvariant();
            }
        }

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;

        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
        {
            var duplicateEmployeeId = await StaffQuery()
                .AnyAsync(u => u.Id != user.Id && u.EmployeeId == request.EmployeeId);

            if (duplicateEmployeeId)
                return Conflict("A staff member with this Employee ID already exists.");

            user.EmployeeId = request.EmployeeId;
        }

        if (request.BranchId.HasValue)
        {
            var branchExists = await _context.Branches.FindAsync(request.BranchId.Value) is not null;
            if (!branchExists)
                return BadRequest($"Branch with ID {request.BranchId} does not exist.");

            user.BranchId = request.BranchId;
        }

        if (request.Active.HasValue)
            user.Active = request.Active.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _outputCache.EvictByTagAsync("staff", default);

        return NoContent();
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateStaffRoleRequest request)
    {
        if (request.Role is not "Admin" and not "Staff")
            return BadRequest($"Invalid role '{request.Role}'. Must be 'Admin' or 'Staff'.");

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Staff") && !roles.Contains("Admin"))
            return NotFound("User is not a staff member.");

        if (roles.Contains(request.Role))
            return Conflict($"User is already in the '{request.Role}' role.");

        var staffRoles = roles.Where(r => r is "Admin" or "Staff").ToList();
        await _userManager.RemoveFromRolesAsync(user, staffRoles);
        await _userManager.AddToRoleAsync(user, request.Role);

        await _outputCache.EvictByTagAsync("staff", default);

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
        if (!roles.Contains("Staff") && !roles.Contains("Admin"))
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _outputCache.EvictByTagAsync("staff", default);

        return NoContent();
    }

    private static IQueryable<ApplicationUser> ApplySorting(
    IQueryable<ApplicationUser> query, string? sortBy, string? sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "firstname" => descending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "lastname" => descending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            "email" => descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "employeeid" => descending ? query.OrderByDescending(u => u.EmployeeId) : query.OrderBy(u => u.EmployeeId),
            _ => descending ? query.OrderByDescending(u => u.HireDate) : query.OrderBy(u => u.HireDate),
        };
    }
}