// ============================================================
// SubscriptionController.cs — Manages member subscriptions.
// Subscriptions link a member to a membership tier (e.g. Gold).
// They start as "Pending" and become "Active" after payment.
// Admin/Staff manage all subscriptions; members use "my" endpoints.
// ============================================================

using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using GymSystem.Shared.Enums;
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
public class SubscriptionController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOutputCacheStore _outputCache;

    public SubscriptionController(GymDbContext context, UserManager<ApplicationUser> userManager, IOutputCacheStore outputCache)
    {
        _context = context;
        _userManager = userManager;
        _outputCache = outputCache;
    }

    // GET api/subscription — List all subscriptions with search, filters, and pagination.
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll([FromQuery] SubscriptionSearchRequest request)
    {
        var query = _context.Subscriptions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(s =>
                s.TierName.ToLower().Contains(term) ||
                s.User.FirstName.ToLower().Contains(term) ||
                s.User.LastName.ToLower().Contains(term));
        }

        if (request.State.HasValue)
            query = query.Where(s => (int)s.State == request.State.Value);

        if (!string.IsNullOrWhiteSpace(request.TierName))
            query = query.Where(s => s.TierName == request.TierName);

        if (request.StartFrom.HasValue)
            query = query.Where(s => s.StartDate >= request.StartFrom.Value);

        if (request.StartTo.HasValue)
            query = query.Where(s => s.StartDate <= request.StartTo.Value);

        var descending = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = request.SortBy?.ToLower() switch
        {
            "membername" => descending ? query.OrderByDescending(s => s.User.LastName) : query.OrderBy(s => s.User.LastName),
            "tiername"   => descending ? query.OrderByDescending(s => s.TierName)      : query.OrderBy(s => s.TierName),
            "state"      => descending ? query.OrderByDescending(s => s.State)         : query.OrderBy(s => s.State),
            "enddate"    => descending ? query.OrderByDescending(s => s.EndDate)        : query.OrderBy(s => s.EndDate),
            _            => descending ? query.OrderByDescending(s => s.StartDate)      : query.OrderBy(s => s.StartDate),
        };

        var result = await query
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
            .ToPagedResultAsync(request.Page, request.PageSize);

        return Ok(result);
    }

    // GET api/subscription/{id} — Get a single subscription by ID.

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

    // Creates a pending subscription. The subscription becomes active
    // only after a payment is recorded via the PaymentController.
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

        await _outputCache.EvictByTagAsync("subscriptions", default);

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

    // PUT api/subscription/{id} — Update a subscription's tier, state, or dates.
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
        await _outputCache.EvictByTagAsync("subscriptions", default);
        return NoContent();
    }

    // DELETE api/subscription/{id} — Delete a subscription.
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

        await _outputCache.EvictByTagAsync("subscriptions", default);

        return NoContent();
    }

    // GET api/subscription/my — A member views their own subscriptions.
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

    // POST api/subscription/my — A member creates a pending subscription for themselves.
    // It stays "Pending" until a payment is made via the PaymentController.
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

        await _outputCache.EvictByTagAsync("subscriptions", default);

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
