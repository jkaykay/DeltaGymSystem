using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using GymSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOutputCacheStore _outputCache;

        public PaymentController(GymDbContext context, UserManager<ApplicationUser> userManager, IOutputCacheStore outputCache)
        {
            _context = context;
            _userManager = userManager;
            _outputCache = outputCache;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Pay([FromBody] AddPaymentRequest request)
        {
            return await ProcessPaymentAsync(request.UserId, request.SubId, request.Amount);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [OutputCache(PolicyName = "payments")]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _context.Payments
                .Select(p => new PaymentDTO
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    UserId = p.UserId,
                    SubId = p.SubId
                })
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [OutputCache(PolicyName = "payments")]
        public async Task<IActionResult> Get(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
                return NotFound();

            return Ok(new PaymentDTO
            {
                PaymentId = payment.PaymentId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                UserId = payment.UserId,
                SubId = payment.SubId
            });
        }

        // PUT endpoint removed — payments are immutable financial records.
        // To correct a mistake: DELETE the incorrect payment, then create
        // a new one through the validated Pay endpoint.

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
                return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            await _outputCache.EvictByTagAsync("payments", default);
            return NoContent();
        }

        // GetMy is intentionally not cached — the "payments" policy has no
        // user-aware vary strategy, so caching here would serve one member's
        // payment history to another. Same reasoning as BookingController.GetMy.
        [HttpGet("my")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMy()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .Select(p => new PaymentDTO
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    UserId = p.UserId,
                    SubId = p.SubId
                })
                .ToListAsync();

            return Ok(payments);
        }

        [HttpPost("my")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> PayMy([FromBody] AddMyPaymentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return await ProcessPaymentAsync(userId, request.SubId, request.Amount);
        }

        // --- Shared logic ---

        private async Task<IActionResult> ProcessPaymentAsync(string userId, int subId, decimal amount)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound("User does not exist.");

            var sub = await _context.Subscriptions
                .Include(s => s.Tier)
                .FirstOrDefaultAsync(s => s.SubId == subId && s.UserId == userId);

            if (sub is null)
                return BadRequest("Subscription does not exist for this user.");

            if (amount != sub.Tier.Price)
                return BadRequest($"Amount must match the tier price of {sub.Tier.Price:C}.");

            if (sub.State == SubscriptionState.Expired)
                return BadRequest("Cannot pay against an expired subscription. Please create a new one.");

            Subscription targetSub;

            if (sub.State == SubscriptionState.Active || sub.State == SubscriptionState.Queued)
            {
                var latestEndDate = await _context.Subscriptions
                    .Where(s => s.UserId == sub.UserId
                             && s.TierName == sub.TierName
                             && (s.State == SubscriptionState.Active || s.State == SubscriptionState.Queued))
                    .MaxAsync(s => s.EndDate);

                var newSub = new Subscription
                {
                    State = SubscriptionState.Queued,
                    UserId = sub.UserId,
                    TierName = sub.TierName,
                    StartDate = latestEndDate,
                    EndDate = latestEndDate.AddMonths(1),
                    User = user,
                    Tier = sub.Tier
                };

                _context.Subscriptions.Add(newSub);
                await _context.SaveChangesAsync();

                targetSub = newSub;
            }
            else
            {
                sub.State = SubscriptionState.Active;
                sub.StartDate = DateTime.UtcNow;
                sub.EndDate = DateTime.UtcNow.AddMonths(1);

                if (!user.Active)
                {
                    user.Active = true;
                    await _userManager.UpdateAsync(user);
                }

                targetSub = sub;
            }

            var payment = new Payment
            {
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                UserId = userId,
                User = user,
                SubId = targetSub.SubId,
                Subscription = targetSub
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Evict both Pay (Admin/Staff) and PayMy (Member) paths —
            // both funnel here so eviction is guaranteed in one place
            await _outputCache.EvictByTagAsync("payments", default);

            return CreatedAtAction(nameof(Get), new { id = payment.PaymentId }, new PaymentDTO
            {
                PaymentId = payment.PaymentId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                UserId = payment.UserId,
                SubId = payment.SubId
            });
        }
    }
}