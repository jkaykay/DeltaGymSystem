using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Staff")]
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
    public async Task<IActionResult> GetAll()
    {
        var subscriptions = await _context.Subscriptions
            .Include(s => s.Payments)
            .Select(s => new SubscriptionDTO
            {
                SubId = s.SubId,
                Status = s.Status,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TierName = s.TierName,
                UserId = s.UserId,
                MemberName = $"{s.User.FirstName} {s.User.LastName}"
            })
            .ToListAsync();

        return Ok(subscriptions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var sub = await _context.Subscriptions
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.SubId == id);

        if (sub is null)
            return NotFound();

        return Ok(new SubscriptionDTO
        {
            SubId = sub.SubId,
            Status = sub.Status,
            StartDate = sub.StartDate,
            EndDate = sub.EndDate,
            TierName = sub.TierName,
            UserId = sub.UserId,
            MemberName = $"{sub.User.FirstName} {sub.User.LastName}"
        });
    }

    /// <summary>
    /// Creates an inactive subscription. The subscription becomes active
    /// only after a payment is recorded via the PaymentController.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddSubscriptionRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return NotFound("User does not exist.");

        var tier = await _context.Tiers.FindAsync(request.TierName);
        if (tier is null)
            return BadRequest($"Tier '{request.TierName}' does not exist.");

        // Prevent duplicate inactive subscriptions for the same user + tier
        var exists = await _context.Subscriptions
            .AnyAsync(s => s.UserId == request.UserId
                        && s.TierName == request.TierName
                        && s.Status == false);

        if (exists)
            return Conflict("An unpaid subscription for this tier already exists for the user.");

        var sub = new Subscription
        {
            Status = false,
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
            Status = sub.Status,
            StartDate = sub.StartDate,
            EndDate = sub.EndDate,
            TierName = sub.TierName,
            UserId = sub.UserId,
            MemberName = $"{sub.User.FirstName} {sub.User.LastName}"
        });
    }

    [HttpPut("{id}")]
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

        if (request.Status.HasValue)
            sub.Status = request.Status.Value;

        if (request.StartDate.HasValue)
            sub.StartDate = request.StartDate.Value;

        if (request.EndDate.HasValue)
            sub.EndDate = request.EndDate.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
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
}