// ============================================================
// RoomController.cs — CRUD endpoints for gym rooms.
// Rooms belong to branches and host sessions. Admin can create,
// update, and delete rooms; Staff and Trainers can view them.
// ============================================================

using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
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
    [Authorize(Roles = "Admin,Staff,Trainer")]
    public class RoomController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly IOutputCacheStore _outputCache;

        public RoomController(GymDbContext context, IOutputCacheStore outputCache)
        {
            _context = context;
            _outputCache = outputCache;
        }

        // GET api/room — List all rooms with optional branch/number filters and pagination.
        [HttpGet]
        [OutputCache(PolicyName = "rooms")]
        public async Task<IActionResult> GetAll([FromQuery] RoomSearchRequest request)
        {
            var query = _context.Rooms.AsQueryable();

            if (request.BranchId.HasValue)
                query = query.Where(r => r.BranchId == request.BranchId.Value);

            if (request.RoomNumber.HasValue)
                query = query.Where(r => r.RoomNumber == request.RoomNumber.Value);

            var descending = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = request.SortBy?.ToLower() switch
            {
                "branch"    => descending ? query.OrderByDescending(r => r.BranchId)        : query.OrderBy(r => r.BranchId),
                "capacity"  => descending ? query.OrderByDescending(r => r.MaxCapacity)     : query.OrderBy(r => r.MaxCapacity),
                "sessions"  => descending ? query.OrderByDescending(r => r.Sessions.Count)  : query.OrderBy(r => r.Sessions.Count),
                "equipment" => descending ? query.OrderByDescending(r => r.Equipments.Count): query.OrderBy(r => r.Equipments.Count),
                _           => descending ? query.OrderByDescending(r => r.RoomNumber)      : query.OrderBy(r => r.RoomNumber),
            };

            var result = await query
                .Select(r => new RoomDTO
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    BranchId = r.BranchId,
                    MaxCapacity = r.MaxCapacity,
                    SessionCount = r.Sessions.Count,
                    EquipmentCount = r.Equipments.Count
                })
                .ToPagedResultAsync(request.Page, request.PageSize);

            return Ok(result);
        }

        // GET api/room/{id} — Get a single room by ID.
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

        // POST api/room — Create a new room in a branch (Admin only).
        // Room numbers must be unique within a branch.
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

        // PUT api/room/{id} — Update room details (Admin only).
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRequest roomRequest)
        {
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

        // DELETE api/room/{id} — Delete a room (Admin only).
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

        // GET api/room/total — Returns the total number of rooms.
        [HttpGet("total")]
        [OutputCache(PolicyName = "rooms")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _context.Rooms.CountAsync();
            return Ok(new CountResponse { Count = total });
        }
    }
}