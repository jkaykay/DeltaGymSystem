using GymSystem.Api.Data;
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
public class TrainerController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GymDbContext _context;
    private readonly IOutputCacheStore _outputCache;

    public TrainerController(UserManager<ApplicationUser> userManager, GymDbContext context, IOutputCacheStore outputCache)
    {
        _userManager = userManager;
        _context = context;
        _outputCache = outputCache;
    }

    private IQueryable<ApplicationUser> TrainersQuery()
    {
        return _context.Users
            .Where(u => _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .Any(x => x.UserId == u.Id && x.Name == "Trainer"));
    }

    [HttpGet]
    [OutputCache(PolicyName = "trainers")]
    public async Task<IActionResult> GetAll()
    {
        var result = await TrainersQuery()
            .Select(m => new UserDTO
            {
                Id = m.Id,
                Email = m.Email!,
                UserName = m.UserName!,
                FirstName = m.FirstName,
                LastName = m.LastName,
                JoinDate = m.JoinDate,
                Active = m.Active,
                BranchId = m.BranchId
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("total")]
    [OutputCache(PolicyName = "trainers")]
    public async Task<IActionResult> GetTotal()
    {
        var count = await TrainersQuery().CountAsync();
        return Ok(new CountResponse { Count = count });
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

        return Ok(new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            JoinDate = user.JoinDate,
            Active = user.Active,
            BranchId = user.BranchId
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTrainerRequest request)
    {
        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
            return Conflict("A user with this email already exists.");

        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
        {
            var duplicateEmployeeId = await TrainersQuery()
                .AnyAsync(u => u.EmployeeId == request.EmployeeId);

            if (duplicateEmployeeId)
                return Conflict("A trainer with this Employee ID already exists.");
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

        await _userManager.AddToRoleAsync(user, "Trainer");

        await _outputCache.EvictByTagAsync("trainers", default);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmployeeId = user.EmployeeId,
            HireDate = user.HireDate,
            Active = user.Active,
            BranchId = user.BranchId
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

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null && existing.Id != user.Id)
                return Conflict("A user with this email already exists.");

            user.Email = request.Email;

            if (string.IsNullOrWhiteSpace(user.EmployeeId))
            {
                user.UserName = request.Email.Split('@')[0];
            }
        }

        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
        {
            var duplicateEmployeeId = await TrainersQuery()
                .AnyAsync(u => u.Id != user.Id && u.EmployeeId == request.EmployeeId);

            if (duplicateEmployeeId)
                return Conflict("A trainer with this Employee ID already exists.");
        }

        if (request.BranchId.HasValue)
        {
            var branchExists = await _context.Branches.FindAsync(request.BranchId.Value) is not null;
            if (!branchExists)
                return BadRequest($"Branch with ID {request.BranchId} does not exist.");

            user.BranchId = request.BranchId;
        }

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.EmployeeId = request.EmployeeId ?? user.EmployeeId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _outputCache.EvictByTagAsync("trainers", default);

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
        if (!roles.Contains("Trainer"))
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _outputCache.EvictByTagAsync("trainers", default);

        return NoContent();
    }
}