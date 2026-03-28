using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class TierController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly IOutputCacheStore _outputCache;

    public TierController(GymDbContext context, IOutputCacheStore outputCache)
    {
        _context = context;
        _outputCache = outputCache;
    }

    [HttpGet]
    [OutputCache(PolicyName = "tiers")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _context.Tiers
            .Select(t => new TierDTO
            {
                TierName = t.TierName,
                Price = t.Price,
                SubCount = t.Subscriptions.Count
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{tierName}")]
    [OutputCache(PolicyName = "tiers")]
    public async Task<IActionResult> Get(string tierName)
    {
        var result = await _context.Tiers
            .Where(t => t.TierName == tierName)
            .Select(t => new TierDTO
            {
                TierName = t.TierName,
                Price = t.Price,
                SubCount = t.Subscriptions.Count
            })
            .FirstOrDefaultAsync();

        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{tierName}")]
    public async Task<IActionResult> Delete(string tierName)
    {
        var tier = await _context.Tiers.FindAsync(tierName);
        if (tier == null) return NotFound();

        _context.Tiers.Remove(tier);
        var rowsAffected = await _context.SaveChangesAsync();
        if (rowsAffected == 0)
            return BadRequest("Failed to delete tier.");

        await _outputCache.EvictByTagAsync("tiers", default);

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddTierRequest request)
    {
        var exists = await _context.Tiers.FindAsync(request.TierName);
        if (exists != null) return Conflict($"Tier {request.TierName} already exists.");

        var tier = new Tier
        {
            TierName = request.TierName,
            Price = request.Price
        };

        _context.Tiers.Add(tier);
        var rowsAffected = await _context.SaveChangesAsync();
        if (rowsAffected == 0) return BadRequest("Failed to create tier.");

        await _outputCache.EvictByTagAsync("tiers", default);

        return CreatedAtAction(nameof(Get), new { TierName = tier.TierName }, new TierDTO
        {
            TierName = tier.TierName,
            Price = tier.Price,
            SubCount = 0
        });
    }

    [HttpPut("{tierName}")]
    public async Task<IActionResult> Update(string tierName, [FromBody] UpdateTierRequest request)
    {
        var tier = await _context.Tiers
            .Include(t => t.Subscriptions)
            .FirstOrDefaultAsync(t => t.TierName == tierName);

        if (tier == null) return NotFound();

        if (request.Price != null) tier.Price = (decimal)request.Price;

        await _context.SaveChangesAsync();

        await _outputCache.EvictByTagAsync("tiers", default);

        return Ok(new TierDTO
        {
            TierName = tier.TierName,
            Price = tier.Price,
            SubCount = tier.Subscriptions.Count
        });
    }
}