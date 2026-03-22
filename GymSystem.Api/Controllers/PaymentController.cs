using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
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

            // Optional: validate the amount matches the tier price
            if (request.Amount != sub.Tier.Price)
                return BadRequest($"Amount must match the tier price of {sub.Tier.Price:C}.");

            int targetSubId;

            if (sub.Status)
            {
                // --- Prepay: subscription is already active, create a follow-on ---
                var newSub = new Subscription
                {
                    Status = true,
                    UserId = sub.UserId,
                    TierName = sub.TierName,
                    StartDate = sub.EndDate,                  // starts when current ends
                    EndDate = sub.EndDate.AddMonths(1),
                    User = user,
                    Tier = sub.Tier
                };

                _context.Subscriptions.Add(newSub);
                await _context.SaveChangesAsync();            // generates newSub.SubId

                targetSubId = newSub.SubId;
            }
            else
            {
                // --- New payment: activate the inactive subscription ---
                sub.Status = true;
                sub.StartDate = DateTime.UtcNow;
                sub.EndDate = DateTime.UtcNow.AddMonths(1);

                targetSubId = sub.SubId;
            }

            var payment = new Payment
            {
                Amount = request.Amount,
                PaymentDate = DateTime.UtcNow,
                UserId = request.UserId,
                User = user,
                SubId = targetSubId,
                Subscription = sub.Status ? sub : await _context.Subscriptions.FindAsync(targetSubId)
                               ?? throw new InvalidOperationException("Subscription not found after save.")
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
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
                return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}