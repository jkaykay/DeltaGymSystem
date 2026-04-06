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
    [Authorize(Roles = "Admin,Staff")]
    public class EquipmentController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly IOutputCacheStore _outputCache;

        public EquipmentController(GymDbContext context, IOutputCacheStore outputCache)
        {
            _context = context;
            _outputCache = outputCache;
        }

        [HttpGet]
        [OutputCache(PolicyName = "equipment")]
        public async Task<IActionResult> GetAll([FromQuery] EquipmentSearchRequest request)
        {
            var query = _context.Equipments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();
                query = query.Where(e =>
                    (e.Description != null && e.Description.ToLower().Contains(term)));
            }

            if (request.Operational.HasValue)
                query = query.Where(e => e.Operational == request.Operational.Value);

            if (request.RoomId.HasValue)
                query = query.Where(e => e.RoomId == request.RoomId.Value);

            if (request.DateFrom.HasValue)
                query = query.Where(e => e.InDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(e => e.InDate <= request.DateTo.Value);

            var descending = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = request.SortBy?.ToLower() switch
            {
                "description" => descending ? query.OrderByDescending(e => e.Description) : query.OrderBy(e => e.Description),
                "status"      => descending ? query.OrderByDescending(e => e.Operational)  : query.OrderBy(e => e.Operational),
                "room"        => descending ? query.OrderByDescending(e => e.Room.RoomNumber) : query.OrderBy(e => e.Room.RoomNumber),
                _             => descending ? query.OrderByDescending(e => e.InDate)       : query.OrderBy(e => e.InDate),
            };

            var result = await query
                .Select(e => new EquipmentDTO
                {
                    EquipmentId = e.EquipmentId,
                    Description = e.Description,
                    InDate = e.InDate,
                    Operational = e.Operational,
                    RoomId = e.RoomId,
                    RoomNumber = e.Room.RoomNumber
                })
                .ToPagedResultAsync(request.Page, request.PageSize);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [OutputCache(PolicyName = "equipment")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _context.Equipments
                .Where(e => e.EquipmentId == id)
                .Select(e => new EquipmentDTO
                {
                    EquipmentId = e.EquipmentId,
                    Description = e.Description,
                    InDate = e.InDate,
                    Operational = e.Operational,
                    RoomId = e.RoomId,
                    RoomNumber = e.Room.RoomNumber
                })
                .FirstOrDefaultAsync();

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AddEquipmentRequest request)
        {
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room == null)
                return BadRequest("Room not found.");

            var equipment = new Equipment
            {
                Description = request.Description,
                InDate = request.InDate,
                Operational = request.Operational,
                RoomId = request.RoomId,
                Room = room
            };

            _context.Equipments.Add(equipment);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to create equipment.");

            await _outputCache.EvictByTagAsync("equipment", default);

            return CreatedAtAction(nameof(Get), new { id = equipment.EquipmentId }, new EquipmentDTO
            {
                EquipmentId = equipment.EquipmentId,
                Description = equipment.Description,
                InDate = equipment.InDate,
                Operational = equipment.Operational,
                RoomId = equipment.RoomId,
                RoomNumber = room.RoomNumber
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipmentRequest request)
        {
            var equipment = await _context.Equipments
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null) return NotFound();

            if (request.Description is not null)
                equipment.Description = request.Description;

            if (request.InDate.HasValue)
                equipment.InDate = request.InDate.Value;

            if (request.Operational.HasValue)
                equipment.Operational = request.Operational.Value;

            if (request.RoomId.HasValue)
            {
                var room = await _context.Rooms.FindAsync(request.RoomId.Value);
                if (room == null)
                    return BadRequest("Room not found.");

                equipment.RoomId = request.RoomId.Value;
                equipment.Room = room;
            }

            await _context.SaveChangesAsync();

            await _outputCache.EvictByTagAsync("equipment", default);

            return Ok(new EquipmentDTO
            {
                EquipmentId = equipment.EquipmentId,
                Description = equipment.Description,
                InDate = equipment.InDate,
                Operational = equipment.Operational,
                RoomId = equipment.RoomId,
                RoomNumber = equipment.Room.RoomNumber
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null) return NotFound();

            _context.Equipments.Remove(equipment);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Failed to delete equipment.");

            await _outputCache.EvictByTagAsync("equipment", default);

            return NoContent();
        }

        [HttpGet("total")]
        [OutputCache(PolicyName = "equipment")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _context.Equipments.CountAsync();
            return Ok(new CountResponse { Count = total });
        }
    }
}
