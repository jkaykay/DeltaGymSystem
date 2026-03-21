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
            if (user == null) return NotFound("User does not exist.");

            var sub = await _context.Subscriptions.FirstOrDefaultAsync(p => p.SubId == request.SubId && p.UserId == request.UserId);
            if (sub == null) return BadRequest("Subscription does not exist.");
            else if(sub.Status == true)
            {
                sub = new Subscription
                { 
                    Status = true,
                    UserId = sub.UserId,
                    User = user,
                    TierName = sub.TierName,

                };
            }

            var payment = new Payment
            {

            };
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id)
        {

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            return NoContent();
        }
    }
}