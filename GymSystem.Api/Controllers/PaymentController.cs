// ============================================================
// PaymentController.cs — Manages subscription payments.
// Admin/Staff can create and view payments for any member.
// Members can make and view their own payments via "my" endpoints.
// Payments are immutable — they cannot be edited after creation.
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
using Microsoft.AspNetCore.RateLimiting;
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

        // POST api/payment — Admin/Staff records a payment for a member.
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Pay([FromBody] AddPaymentRequest request)
        {
            return await ProcessPaymentAsync(request.UserId, request.SubId, request.Amount);
        }

        // GET api/payment — List all payments with search, filters, and pagination.
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [OutputCache(PolicyName = "payments")]
        public async Task<IActionResult> GetAll([FromQuery] PaymentSearchRequest request)
        {
            var query = _context.Payments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();
                query = query.Where(p =>
                    p.User.FirstName.ToLower().Contains(term) ||
                    p.User.LastName.ToLower().Contains(term) ||
                    p.User.Email!.ToLower().Contains(term));
            }

            if (request.DateFrom.HasValue)
                query = query.Where(p => p.PaymentDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(p => p.PaymentDate <= request.DateTo.Value);

            if (request.MinAmount.HasValue)
                query = query.Where(p => p.Amount >= request.MinAmount.Value);

            if (request.MaxAmount.HasValue)
                query = query.Where(p => p.Amount <= request.MaxAmount.Value);

            var descending = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = request.SortBy?.ToLower() switch
            {
                "amount" => descending ? query.OrderByDescending(p => p.Amount)      : query.OrderBy(p => p.Amount),
                "member" => descending ? query.OrderByDescending(p => p.User.LastName) : query.OrderBy(p => p.User.LastName),
                "subid"  => descending ? query.OrderByDescending(p => p.SubId)       : query.OrderBy(p => p.SubId),
                _        => descending ? query.OrderByDescending(p => p.PaymentDate) : query.OrderBy(p => p.PaymentDate),
            };

            var payments = await query
                .Select(p => new PaymentDTO
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    UserId = p.UserId,
                    UserFullName = p.User.FirstName + " " + p.User.LastName,
                    SubId = p.SubId
                })
                .ToPagedResultAsync(request.Page, request.PageSize);

            return Ok(payments);
        }

        // GET api/payment/{id} — Get a single payment by ID.
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

        // DELETE api/payment/{id} — Delete a payment record (Admin only).
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

        // GET api/payment/my — A member views their own payment history.
        // GetMy is intentionally not cached — the "payments" policy has no
        // user-aware vary strategy, so caching here would serve one member's
        // payment history to another. Same reasoning as BookingController.GetMy.
        [HttpGet("my")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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
                .ToPagedResultAsync(page, pageSize);

            return Ok(payments);
        }

        // POST api/payment/my — A member makes a payment for their own subscription.
        [HttpPost("my")]
        [Authorize(Roles = "Member")]
        [EnableRateLimiting("payment")]
        public async Task<IActionResult> PayMy([FromBody] AddMyPaymentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return await ProcessPaymentAsync(userId, request.SubId, request.Amount);
        }

        // --- Shared logic ---

        // Core payment logic shared by both admin and member endpoints.
        // Validates the user, subscription, and amount. If the subscription is
        // already active, a new "queued" subscription is created for the next month.
        // If pending, the subscription is activated immediately.
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