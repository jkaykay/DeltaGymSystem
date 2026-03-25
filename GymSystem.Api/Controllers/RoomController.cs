using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class RoomController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly IOutputCacheStore _outputCache;

        public RoomController(GymDbContext context, IOutputCacheStore outputCache)
        {
            _context = context;
            _outputCache = outputCache;
        }

        [HttpGet]
        [OutputCache(PolicyName = "rooms")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.Rooms
                .Select(r => new RoomDTO
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    BranchId = r.BranchId,
                    MaxCapacity = r.MaxCapacity,
                    SessionCount = r.Sessions.Count,
                    EquipmentCount = r.Equipments.Count
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [OutputCache(PolicyName = "rooms")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _context.Rooms
                .Where(r => r.RoomId == id)
                .Select(r => new RoomDTO
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    BranchId = r.BranchId,
                    MaxCapacity = r.MaxCapacity,
                    SessionCount = r.Sessions.Count,
                    EquipmentCount = r.Equipments.Count
                })
                .FirstOrDefaultAsync();

            if (result == null) return NotFound();

            return Ok(result);
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

            await _outputCache.EvictByTagAsync("rooms", default);

            return CreatedAtAction(nameof(Get), new { id = room.RoomId }, new RoomDTO
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                BranchId = room.BranchId,
                MaxCapacity = room.MaxCapacity,
                SessionCount = 0,
                EquipmentCount = 0
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRequest roomRequest)
        {
            // Load room with counts in a single query
            var roomData = await _context.Rooms
                .Where(r => r.RoomId == id)
                .Select(r => new
                {
                    Room = r,
                    SessionCount = r.Sessions.Count,
                    EquipmentCount = r.Equipments.Count
                })
                .FirstOrDefaultAsync();

            if (roomData == null) return NotFound();

            var room = roomData.Room;

            if (roomRequest.BranchId.HasValue)
            {
                var branch = await _context.Branches.FindAsync(roomRequest.BranchId.Value);
                if (branch == null)
                    return BadRequest("Branch not found");

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
                return Conflict($"Room number {room.RoomNumber} already exists in branch {room.BranchId}.");

            await _context.SaveChangesAsync();

            await _outputCache.EvictByTagAsync("rooms", default);

            return Ok(new RoomDTO
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                BranchId = room.BranchId,
                MaxCapacity = room.MaxCapacity,
                SessionCount = roomData.SessionCount,
                EquipmentCount = roomData.EquipmentCount
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

            await _outputCache.EvictByTagAsync("rooms", default);

            return NoContent();
        }

        [HttpGet("total")]
        [OutputCache(PolicyName = "rooms")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _context.Rooms.CountAsync();
            return Ok(new CountResponse { Count = total });
        }
    }
}