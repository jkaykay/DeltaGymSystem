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
    public class RoomController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoomController(GymDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _context.Rooms.ToListAsync();
            var result = rooms.Select(r => new RoomDTO
            {
                RoomId = r.RoomId,
                BranchId = r.BranchId,
                MaxCapacity = r.MaxCapacity,
                SessionCount = r.Sessions.Count
            }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            return Ok(new RoomDTO
            {
                RoomId = room.RoomId,
                BranchId = room.BranchId,
                MaxCapacity = room.MaxCapacity,
                SessionCount = room.Sessions.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddRoomRequest roomRequest)
        {
            var branch = await _context.Branches.FindAsync(roomRequest.BranchId);
            if (branch == null)
            {
                return BadRequest("Branch not found");
            }

            var room = new Room
            {
                RoomNumber = roomRequest.RoomNumber,
                BranchId = roomRequest.BranchId,
                Branch = branch,
                MaxCapacity = roomRequest.MaxCapacity,
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = room.RoomId }, new RoomDTO 
            { 
                RoomId = room.RoomId,
                BranchId = room.BranchId,
                MaxCapacity = room.MaxCapacity,
                SessionCount = room.Sessions.Count
            });
        }
    }
}
