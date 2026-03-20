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
                SessionCount = r.Sessions.Count,
                EquipmentCount = r.Equipments.Count
            }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            return Ok(new RoomDTO
            {
                RoomId = room.RoomId,
                BranchId = room.BranchId,
                MaxCapacity = room.MaxCapacity,
                SessionCount = room.Sessions.Count,
                EquipmentCount = room.Equipments.Count
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AddRoomRequest roomRequest)
        {
            var branch = await _context.Branches.FindAsync(roomRequest.BranchId);
            if (branch == null)
            {
                return BadRequest("Branch not found");
            }

            var exists = await _context.Rooms.AnyAsync(r =>
                r.RoomNumber == roomRequest.RoomNumber && r.BranchId == roomRequest.BranchId);
            if (exists)
            {
                return Conflict($"Room number {roomRequest.RoomNumber} already exists in branch {roomRequest.BranchId}.");
            }

            var room = new Room
            {
                RoomNumber = roomRequest.RoomNumber,
                BranchId = roomRequest.BranchId,
                Branch = branch,
                MaxCapacity = roomRequest.MaxCapacity,
            };
            _context.Rooms.Add(room);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to create room.");

            return CreatedAtAction(nameof(Get), new { id = room.RoomId }, new RoomDTO
            {
                RoomId = room.RoomId,
                BranchId = room.BranchId,
                MaxCapacity = room.MaxCapacity,
                SessionCount = room.Sessions.Count,
                EquipmentCount = room.Equipments.Count
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRequest roomRequest)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            if (roomRequest.BranchId.HasValue)
            {
                var branch = await _context.Branches.FindAsync(roomRequest.BranchId.Value);
                if (branch == null)
                {
                    return BadRequest("Branch not found");
                }

                room.BranchId = roomRequest.BranchId.Value;
                room.Branch = branch;
            }

            if (roomRequest.RoomNumber.HasValue)
                room.RoomNumber = roomRequest.RoomNumber.Value;

            if (roomRequest.MaxCapacity.HasValue)
                room.MaxCapacity = roomRequest.MaxCapacity.Value;

            var duplicateExists = await _context.Rooms.AnyAsync(r =>
                r.RoomId != id && r.RoomNumber == room.RoomNumber && r.BranchId == room.BranchId);
            if (duplicateExists)
            {
                return Conflict($"Room number {room.RoomNumber} already exists in branch {room.BranchId}.");
            }

            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to update room.");

            return Ok(new RoomDTO
            {
                RoomId = room.RoomId,
                BranchId = room.BranchId,
                MaxCapacity = room.MaxCapacity,
                SessionCount = room.Sessions.Count,
                EquipmentCount = room.Equipments.Count
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            _context.Rooms.Remove(room);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to delete room.");

            return NoContent();
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _context.Rooms.CountAsync();
            return Ok(new CountResponse { Count = total });
        }
    }
}