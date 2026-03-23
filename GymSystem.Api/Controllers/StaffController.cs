using GymSystem.Api.Data;
using GymSystem.Shared.DTOs;
using GymSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Staff")]
public class StaffController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GymDbContext _context;

    public StaffController(UserManager<ApplicationUser> userManager, GymDbContext context)
    {
        _userManager = userManager;
        _context = context;
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

        var result = new List<UserDTO>();
        foreach (var s in allStaff)
        {
            var roles = await _userManager.GetRolesAsync(s);
            result.Add(new UserDTO
            {
                Id = s.Id,
                Email = s.Email!,
                UserName = s.UserName!,
                FirstName = s.FirstName,
                LastName = s.LastName,
                HireDate = s.HireDate,
                EmployeeId = s.EmployeeId,
                Active = s.Active,
                BranchId = s.BranchId,
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
        var total = staff
            .Union(admin, new UserIdComparer())
            .Count();
        return Ok(new CountResponse { Count = total });
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
            var allStaff = await _userManager.GetUsersInRoleAsync("Staff");
            var allAdmin = await _userManager.GetUsersInRoleAsync("Admin");
            var duplicateEmployeeId = allStaff.Concat(allAdmin)
                .Any(u => u.EmployeeId == request.EmployeeId);

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

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;

        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
        {
            var allStaff = await _userManager.GetUsersInRoleAsync("Staff");
            var allAdmin = await _userManager.GetUsersInRoleAsync("Admin");
            var duplicateEmployeeId = allStaff.Concat(allAdmin)
                .Any(u => u.Id != user.Id && u.EmployeeId == request.EmployeeId);

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

        return NoContent();
    }

    private sealed class UserIdComparer : IEqualityComparer<ApplicationUser>
    {
        public bool Equals(ApplicationUser? x, ApplicationUser? y) => x?.Id == y?.Id;
        public int GetHashCode(ApplicationUser obj) => obj.Id.GetHashCode();
    }
}