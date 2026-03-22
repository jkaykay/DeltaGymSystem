using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using GymSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public PaymentController(GymDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Pay([FromBody] AddPaymentRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                return NotFound("User does not exist.");

            var sub = await _context.Subscriptions
                .Include(s => s.Tier)
                .FirstOrDefaultAsync(s => s.SubId == request.SubId && s.UserId == request.UserId);

            if (sub is null)
                return BadRequest("Subscription does not exist for this user.");

            // Validate the amount matches the tier price
            if (request.Amount != sub.Tier.Price)
                return BadRequest($"Amount must match the tier price of {sub.Tier.Price:C}.");

            // Cannot pay against an already-expired subscription
            if (sub.State == SubscriptionState.Expired)
                return BadRequest("Cannot pay against an expired subscription. Please create a new one.");

            Subscription targetSub;

            if (sub.State == SubscriptionState.Active || sub.State == SubscriptionState.Queued)
            {
                // --- Prepay: find the latest end date for this user + tier to chain correctly ---
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
                    StartDate = latestEndDate,                // starts when the latest one ends
                    EndDate = latestEndDate.AddMonths(1),
                    User = user,
                    Tier = sub.Tier
                };

                _context.Subscriptions.Add(newSub);
                await _context.SaveChangesAsync();            // generates newSub.SubId

                targetSub = newSub;
            }
            else
            {
                // --- First payment: activate the Pending subscription ---
                sub.State = SubscriptionState.Active;
                sub.StartDate = DateTime.UtcNow;
                sub.EndDate = DateTime.UtcNow.AddMonths(1);

                // Activate the member now that they've paid
                if (!user.Active)
                {
                    user.Active = true;
                    await _userManager.UpdateAsync(user);
                }

                targetSub = sub;
            }

            var payment = new Payment
            {
                Amount = request.Amount,
                PaymentDate = DateTime.UtcNow,
                UserId = request.UserId,
                User = user,
                SubId = targetSub.SubId,
                Subscription = targetSub
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = payment.PaymentId }, new PaymentDTO
            {
                PaymentId = payment.PaymentId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                UserId = payment.UserId,
                SubId = payment.SubId
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, [FromBody] UpdatePaymentRequest request)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
                return NotFound();

            // Apply only the fields the caller supplied (partial update)
            if (request.Amount.HasValue)
                payment.Amount = request.Amount.Value;

            if (request.PaymentDate.HasValue)
                payment.PaymentDate = request.PaymentDate.Value;

            if (request.UserId is not null)
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user is null)
                    return NotFound("User does not exist.");
                payment.UserId = request.UserId;
            }

            if (request.SubId.HasValue)
            {
                var sub = await _context.Subscriptions.FindAsync(request.SubId.Value);
                if (sub is null)
                    return BadRequest("Subscription does not exist.");
                payment.SubId = request.SubId.Value;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
                return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

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
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound("User does not exist.");

            var sub = await _context.Subscriptions
                .Include(s => s.Tier)
                .FirstOrDefaultAsync(s => s.SubId == request.SubId && s.UserId == userId);

            if (sub is null)
                return BadRequest("Subscription does not exist for this user.");

            if (request.Amount != sub.Tier.Price)
                return BadRequest($"Amount must match the tier price of {sub.Tier.Price:C}.");

            // Cannot pay against an already-expired subscription
            if (sub.State == SubscriptionState.Expired)
                return BadRequest("Cannot pay against an expired subscription. Please create a new one.");

            Subscription targetSub;

            if (sub.State == SubscriptionState.Active || sub.State == SubscriptionState.Queued)
            {
                // --- Prepay: chain from the latest end date for this user + tier ---
                var latestEndDate = await _context.Subscriptions
                    .Where(s => s.UserId == userId
                             && s.TierName == sub.TierName
                             && (s.State == SubscriptionState.Active || s.State == SubscriptionState.Queued))
                    .MaxAsync(s => s.EndDate);

                var newSub = new Subscription
                {
                    State = SubscriptionState.Queued,
                    UserId = userId,
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
                // --- First payment: activate the Pending subscription ---
                sub.State = SubscriptionState.Active;
                sub.StartDate = DateTime.UtcNow;
                sub.EndDate = DateTime.UtcNow.AddMonths(1);

                // Activate the member now that they've paid
                if (!user.Active)
                {
                    user.Active = true;
                    await _userManager.UpdateAsync(user);
                }

                targetSub = sub;
            }

            var payment = new Payment
            {
                Amount = request.Amount,
                PaymentDate = DateTime.UtcNow,
                UserId = userId,
                User = user,
                SubId = targetSub.SubId,
                Subscription = targetSub
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMy), new PaymentDTO
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