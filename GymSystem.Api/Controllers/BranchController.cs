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
    public class BranchController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public BranchController(GymDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var branches = await _context.Branches.ToListAsync();
            var result = branches.Select(b => new BranchDTO
            {
                BranchId = b.BranchId,
                Address = b.Address,
                City = b.City,
                Province = b.Province,
                PostCode = b.PostCode,
                OpenDate = b.OpenDate
            }).ToList;

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch is null) return NotFound();

            return Ok(new BranchDTO
            {
                BranchId = branch.BranchId,
                Address = branch.Address,
                City = branch.City,
                Province = branch.Province,
                PostCode = branch.PostCode,
                OpenDate = branch.OpenDate
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddBranchRequest request) 
        {
            return Ok();
        }

    }
}
