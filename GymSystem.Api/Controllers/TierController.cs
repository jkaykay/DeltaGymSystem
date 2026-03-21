using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace GymSystem.Api.Controllers
{
    public class TierController : ControllerBase
    {
        private readonly GymDbContext _context;

        public TierController(GymDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tiers = await _context.Tiers.Include(t => t.Subscriptions).ToListAsync();
            var result = tiers.Select(t => new TierDTO
            {
                TierName = t.TierName,
                Price = t.Price,
                SubCount = t.Subscriptions.Count
            });

            return Ok(result);
        }

        [HttpGet("{tierName}")]
        public async Task<IActionResult> Get(string tierName) 
        {
            var tier = await _context.Tiers.Include(t => t.Subscriptions).FirstOrDefaultAsync(t => t.TierName == tierName);
            if (tier == null) return NotFound();
            return Ok(new TierDTO
            {
                TierName = tier.TierName,
                Price = tier.Price,
                SubCount = tier.Subscriptions.Count
            });
        }

        [HttpDelete("{tierName}")]
        public async Task<IActionResult> Delete(string tierName)
        {
            var tier = await _context.Tiers.FindAsync(tierName);
            if (tier == null) return NotFound();

            _context.Tiers.Remove(tier);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0)
                return BadRequest("Failed to delete branch.");

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
            if (rowsAffected == 0) return BadRequest("Failed to create Tier.");

            return CreatedAtAction(nameof(Get), new { TierName = tier.TierName }, new TierDTO
            {
                TierName = tier.TierName,
                Price = tier.Price,
                SubCount = tier.Subscriptions.Count
            });
        }

        [HttpPut("{tierName}")]
        public async Task<IActionResult> Update(string tierName, [FromBody] UpdateTierRequest request)
        {
            var tier = await _context.Tiers.FindAsync(tierName);
            if (tier == null) return NotFound();

            if (request.TierName != null) tier.TierName = request.TierName;
            if (request.Price != null) tier.Price = (decimal)request.Price;

            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0)
                return BadRequest("Failed to update branch.");

            return Ok(new TierDTO
            {
                TierName = tier.TierName,
                Price = tier.Price,
                SubCount = tier.Subscriptions.Count
            });
        }
    }
}
