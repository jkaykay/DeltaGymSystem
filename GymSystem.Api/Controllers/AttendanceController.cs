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
public class AttendanceController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOutputCacheStore _outputCache;

    public AttendanceController(GymDbContext context, UserManager<ApplicationUser> userManager, IOutputCacheStore outputCache)
    {
        _context = context;
        _userManager = userManager;
        _outputCache = outputCache;
    }

    [HttpGet]
    [Authorize(Roles = "Staff,Admin")]
    [OutputCache(PolicyName = "attendance")]
    public async Task<IActionResult> GetAll([FromQuery] AttendanceSearchRequest request)
    {
        var query = _context.Attendances.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(a =>
                a.User.FirstName.ToLower().Contains(term) ||
                a.User.LastName.ToLower().Contains(term) ||
                a.User.Email!.ToLower().Contains(term));
        }

        if (request.InFlag.HasValue)
            query = query.Where(a => a.InFlag == request.InFlag.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.CheckIn >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(a => a.CheckIn <= request.DateTo.Value);

        var descending = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = request.SortBy?.ToLower() switch
        {
            "member"   => descending ? query.OrderByDescending(a => a.User.LastName) : query.OrderBy(a => a.User.LastName),
            "checkout" => descending ? query.OrderByDescending(a => a.CheckOut)      : query.OrderBy(a => a.CheckOut),
            "status"   => descending ? query.OrderByDescending(a => a.InFlag)        : query.OrderBy(a => a.InFlag),
            _          => descending ? query.OrderByDescending(a => a.CheckIn)       : query.OrderBy(a => a.CheckIn),
        };

        var result = await query
            .Select(a => new AttendanceDTO
            {
                AttendanceId = a.AttendanceId,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                UserId = a.UserId,
                MemberName = $"{a.User.FirstName} {a.User.LastName}",
                InFlag = a.InFlag
            })
            .ToPagedResultAsync(request.Page, request.PageSize);

        return Ok(result);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Staff,Admin")]
    [OutputCache(PolicyName = "attendance")]
    public async Task<IActionResult> GetActive()
    {
        var active = await _context.Attendances
            .Where(a => a.InFlag)
            .Select(a => new AttendanceDTO
            {
                AttendanceId = a.AttendanceId,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                UserId = a.UserId,
                MemberName = $"{a.User.FirstName} {a.User.LastName}",
                InFlag = a.InFlag
            })
            .ToListAsync();

        return Ok(active);
    }

    [HttpGet("{id}")]
    [OutputCache(PolicyName = "attendance")]
    public async Task<IActionResult> GetById(int id)
    {
        var attendance = await _context.Attendances
            .Where(a => a.AttendanceId == id)
            .Select(a => new AttendanceDTO
            {
                AttendanceId = a.AttendanceId,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                UserId = a.UserId,
                MemberName = $"{a.User.FirstName} {a.User.LastName}",
                InFlag = a.InFlag
            })
            .FirstOrDefaultAsync();

        if (attendance == null) return NotFound("Specified attendance record does not exist.");

        if (!IsAdminOrStaff() && !IsSelf(attendance.UserId))
            return Forbid();

        return Ok(attendance);
    }

    [HttpGet("member/{memberId}")]
    [OutputCache(PolicyName = "attendance")]
    public async Task<IActionResult> GetMemberAttendances(string memberId)
    {
        if (!IsAdminOrStaff() && !IsSelf(memberId))
            return Forbid();

        var user = await _userManager.FindByIdAsync(memberId);
        if (user is null) return NotFound($"No user with ID '{memberId}' was found.");

        var memberAttendances = await _context.Attendances
            .Where(a => a.UserId == memberId)
            .Select(a => new AttendanceDTO
            {
                AttendanceId = a.AttendanceId,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                UserId = a.UserId,
                MemberName = $"{a.User.FirstName} {a.User.LastName}",
                InFlag = a.InFlag
            })
            .ToListAsync();

        if (!memberAttendances.Any())
            return Ok(new List<AttendanceDTO>());

        return Ok(memberAttendances);
    }

    [HttpPost("checkin/{memberId}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> CheckIn(string memberId)
    {
        var user = await _userManager.FindByIdAsync(memberId);
        if (user is null) return NotFound($"No user with ID '{memberId}' was found.");

        if (!user.Active) return Forbid();

        var openSession = await _context.Attendances
            .FirstOrDefaultAsync(a => a.UserId == memberId && a.InFlag);

        if (openSession is not null)
            return Conflict("This member already has an active check-in session.");

        var attendance = new Attendance
        {
            CheckIn = DateTime.UtcNow,
            CheckOut = default,
            InFlag = true,
            UserId = memberId,
            User = user
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        await _outputCache.EvictByTagAsync("attendance", default);

        return CreatedAtAction(nameof(GetById), new { id = attendance.AttendanceId }, new AttendanceDTO
        {
            AttendanceId = attendance.AttendanceId,
            CheckIn = attendance.CheckIn,
            CheckOut = attendance.CheckOut,
            UserId = attendance.UserId,
            MemberName = $"{user.FirstName} {user.LastName}",
            InFlag = attendance.InFlag
        });
    }

    [HttpPut("checkout/{memberId}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> CheckOut(string memberId)
    {
        var openSession = await _context.Attendances
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == memberId && a.InFlag);

        if (openSession is null)
            return NotFound("No active check-in session found for this member.");

        openSession.CheckOut = DateTime.UtcNow;
        openSession.InFlag = false;

        await _context.SaveChangesAsync();

        await _outputCache.EvictByTagAsync("attendance", default);

        return Ok(new AttendanceDTO
        {
            AttendanceId = openSession.AttendanceId,
            CheckIn = openSession.CheckIn,
            CheckOut = openSession.CheckOut,
            UserId = openSession.UserId,
            MemberName = $"{openSession.User.FirstName} {openSession.User.LastName}",
            InFlag = openSession.InFlag
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var attendance = await _context.Attendances.FindAsync(id);

        if (attendance is null)
            return NotFound("Specified attendance record does not exist.");

        _context.Attendances.Remove(attendance);
        await _context.SaveChangesAsync();

        await _outputCache.EvictByTagAsync("attendance", default);

        return NoContent();
    }

    private bool IsSelf(string id) =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) == id;

    private bool IsAdminOrStaff() =>
        User.IsInRole("Admin") || User.IsInRole("Staff");
}