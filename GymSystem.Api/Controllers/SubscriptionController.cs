using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using GymSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubscriptionController(GymDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _context.Subscriptions
            .Select(s => new SubscriptionDTO
            {
                SubId = s.SubId,
                State = s.State,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TierName = s.TierName,
                UserId = s.UserId,
                MemberName = $"{s.User.FirstName} {s.User.LastName}"
            })
            .ToPagedResultAsync(page, pageSize);

        return Ok(result);
    }


    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _context.Subscriptions
            .Where(s => s.SubId == id)
            .Select(s => new SubscriptionDTO
            {
                SubId = s.SubId,
                State = s.State,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TierName = s.TierName,
                UserId = s.UserId,
                MemberName = $"{s.User.FirstName} {s.User.LastName}"
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Creates a pending subscription. The subscription becomes active
    /// only after a payment is recorded via the PaymentController.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] AddSubscriptionRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return NotFound("User does not exist.");

        var tier = await _context.Tiers.FindAsync(request.TierName);
        if (tier is null)
            return BadRequest($"Tier '{request.TierName}' does not exist.");

        // Only block if there is already a Pending (unpaid) subscription for the same user + tier.
        // Expired subscriptions no longer interfere with creating a new one.
        var exists = await _context.Subscriptions
            .AnyAsync(s => s.UserId == request.UserId
                        && s.TierName == request.TierName
                        && s.State == SubscriptionState.Pending);

        if (exists)
            return Conflict("An unpaid subscription for this tier already exists for the user.");

        var sub = new Subscription
        {
            State = SubscriptionState.Pending,
            StartDate = default,
            EndDate = default,
            TierName = request.TierName,
            Tier = tier,
            UserId = request.UserId,
            User = user
        };

        _context.Subscriptions.Add(sub);
        var rowsAffected = await _context.SaveChangesAsync();
        if (rowsAffected == 0)
            return BadRequest("Subscription creation failed.");

        return CreatedAtAction(nameof(Get), new { id = sub.SubId }, new SubscriptionDTO
        {
            SubId = sub.SubId,
            State = sub.State,
            StartDate = sub.StartDate,
            EndDate = sub.EndDate,
            TierName = sub.TierName,
            UserId = sub.UserId,
            MemberName = $"{sub.User.FirstName} {sub.User.LastName}"
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Edit(int id, [FromBody] UpdateSubscriptionRequest request)
    {
        var sub = await _context.Subscriptions.FindAsync(id);
        if (sub is null)
            return NotFound();

        if (request.TierName is not null)
        {
            var tier = await _context.Tiers.FindAsync(request.TierName);
            if (tier is null)
                return BadRequest($"Tier '{request.TierName}' does not exist.");
            sub.TierName = request.TierName;
            sub.Tier = tier;
        }

        if (request.State.HasValue)
            sub.State = request.State.Value;

        if (request.StartDate.HasValue)
            sub.StartDate = request.StartDate.Value;

        if (request.EndDate.HasValue)
            sub.EndDate = request.EndDate.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var sub = await _context.Subscriptions.FindAsync(id);
        if (sub is null)
            return NotFound();

        _context.Subscriptions.Remove(sub);
        var rowsAffected = await _context.SaveChangesAsync();
        if (rowsAffected == 0)
            return BadRequest("Subscription deletion failed.");

        return NoContent();
    }

    [HttpGet("my")]
    [Authorize(Roles = "Member")]
    public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var subscriptions = await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => new SubscriptionDTO
            {
                SubId = s.SubId,
                State = s.State,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TierName = s.TierName,
                UserId = s.UserId,
                MemberName = $"{s.User.FirstName} {s.User.LastName}"
            })
            .ToPagedResultAsync(page, pageSize);

        return Ok(subscriptions);
    }

    [HttpPost("my")]
    [Authorize(Roles = "Member")]
    public async Task<IActionResult> CreateMy([FromBody] AddMySubscriptionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound("User does not exist.");

        var tier = await _context.Tiers.FindAsync(request.TierName);
        if (tier is null)
            return BadRequest($"Tier '{request.TierName}' does not exist.");

        // Only block if there is already a Pending (unpaid) subscription for the same tier.
        var exists = await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId
                        && s.TierName == request.TierName
                        && s.State == SubscriptionState.Pending);

        if (exists)
            return Conflict("An unpaid subscription for this tier already exists.");

        var sub = new Subscription
        {
            State = SubscriptionState.Pending,
            StartDate = default,
            EndDate = default,
            TierName = request.TierName,
            Tier = tier,
            UserId = userId,
            User = user
        };

        _context.Subscriptions.Add(sub);
        var rowsAffected = await _context.SaveChangesAsync();
        if (rowsAffected == 0)
            return BadRequest("Subscription creation failed.");

        return CreatedAtAction(nameof(GetMy), null, new SubscriptionDTO
        {
            SubId = sub.SubId,
            State = sub.State,
            StartDate = sub.StartDate,
            EndDate = sub.EndDate,
            TierName = sub.TierName,
            UserId = sub.UserId,
            MemberName = $"{user.FirstName} {user.LastName}"
        });
    }
}