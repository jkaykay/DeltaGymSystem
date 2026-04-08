// ============================================================
// ClassController.cs — CRUD endpoints for gym classes.
// Classes represent subjects (e.g. "Yoga") taught by trainers.
// Public users can view classes; authorized users can manage them.
// ============================================================

using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff,Trainer")]
    public class ClassController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOutputCacheStore _outputCache;

        public ClassController(GymDbContext context, UserManager<ApplicationUser> userManager, IOutputCacheStore outputCache)
        {
            _context = context;
            _userManager = userManager;
            _outputCache = outputCache;
        }

        // GET api/class — List all classes (public — no login required).
        [HttpGet]
        [AllowAnonymous]
        [OutputCache(PolicyName = "classes")]
        public async Task<IActionResult> GetAll([FromQuery] ClassSearchRequest request)
        {
            var query = _context.Classes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.UserId))
                query = query.Where(c => c.UserId == request.UserId);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();
                query = query.Where(c =>
                    c.Subject.ToLower().Contains(term) ||
                    c.User.FirstName.ToLower().Contains(term) ||
                    c.User.LastName.ToLower().Contains(term));
            }

            var descending = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = request.SortBy?.ToLower() switch
            {
                "trainer"  => descending ? query.OrderByDescending(c => c.User.LastName) : query.OrderBy(c => c.User.LastName),
                "sessions" => descending ? query.OrderByDescending(c => c.Sessions.Count) : query.OrderBy(c => c.Sessions.Count),
                _          => descending ? query.OrderByDescending(c => c.Subject)       : query.OrderBy(c => c.Subject),
            };

            var now = DateTime.UtcNow;

            var result = await query
                .Select(c => new ClassDTO
                {
                    ClassId = c.ClassId,
                    Subject = c.Subject,
                    UserId = c.UserId,
                    TrainerName = $"{c.User.FirstName} {c.User.LastName}",
                    SessionCount = c.Sessions.Count,
                    UpcomingSessionCount = c.Sessions.Count(s => s.Start >= now)
                })
                .ToPagedResultAsync(request.Page, request.PageSize);

            return Ok(result);
        }

        // GET api/class/{id} — Get a single class by ID (public).
        [HttpGet("{id}")]
        [AllowAnonymous]
        [OutputCache(PolicyName = "classes")]
        public async Task<IActionResult> GetById(int id)
        {
            var now = DateTime.UtcNow;

            var result = await _context.Classes
                .Where(c => c.ClassId == id)
                .Select(c => new ClassDTO
                {
                    ClassId = c.ClassId,
                    Subject = c.Subject,
                    UserId = c.UserId,
                    TrainerName = $"{c.User.FirstName} {c.User.LastName}",
                    SessionCount = c.Sessions.Count,
                    UpcomingSessionCount = c.Sessions.Count(s => s.Start >= now)
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST api/class — Create a new class. Prevents duplicates (same subject + trainer).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddClassRequest request)
        {
            var exists = await _context.Classes
                .AnyAsync(c => c.UserId == request.UserId && c.Subject == request.Subject);

            if (exists)
            {
                return Conflict("A class with this subject already exists for the specified user.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return BadRequest("Trainer not found.");
            }

            var newClass = new Class
            {
                Subject = request.Subject,
                UserId = request.UserId,
                User = user
            };

            _context.Classes.Add(newClass);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Class creation failed.");

            await _outputCache.EvictByTagAsync("classes", default);

            return CreatedAtAction(nameof(GetById), new { id = newClass.ClassId }, new ClassDTO
            {
                ClassId = newClass.ClassId,
                Subject = newClass.Subject,
                UserId = newClass.UserId,
                TrainerName = $"{newClass.User.FirstName} {newClass.User.LastName}",
                SessionCount = 0
            });
        }

        // PUT api/class/{id} — Update a class's subject or assigned trainer.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassRequest request)
        {
            var now = DateTime.UtcNow;

            var classData = await _context.Classes
                .Include(c => c.User)
                .Where(c => c.ClassId == id)
                .Select(c => new
                {
                    ClassObject = c,
                    SessionCount = c.Sessions.Count,
                    UpcomingSessionCount = c.Sessions.Count(s => s.Start >= now)
                })
                .FirstOrDefaultAsync();

            if (classData == null)
            {
                return NotFound();
            }

            var classEntity = classData.ClassObject;

            var effectiveSubject = request.Subject ?? classEntity.Subject;
            var effectiveUserId = request.UserId ?? classEntity.UserId;

            var exists = await _context.Classes
                .AnyAsync(c => c.UserId == effectiveUserId && c.Subject == effectiveSubject && c.ClassId != id);
            if (exists)
            {
                return Conflict("A class with this subject already exists for the specified user.");
            }

            if (request.UserId is not null)
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return BadRequest("Trainer not found.");
                }
                classEntity.UserId = request.UserId;
                classEntity.User = user;
            }

            if (request.Subject is not null)
            {
                classEntity.Subject = request.Subject;
            }

            await _context.SaveChangesAsync();

            await _outputCache.EvictByTagAsync("classes", default);

            return Ok(new ClassDTO
            {
                ClassId = classEntity.ClassId,
                Subject = classEntity.Subject,
                UserId = classEntity.UserId,
                TrainerName = $"{classEntity.User.FirstName} {classEntity.User.LastName}",
                SessionCount = classData.SessionCount,
                UpcomingSessionCount = classData.UpcomingSessionCount
            });
        }

        // DELETE api/class/{id} — Delete a class and all its sessions.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var classEntity = await _context.Classes.FindAsync(id);
            if (classEntity == null)
            {
                return NotFound();
            }
            _context.Classes.Remove(classEntity);
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0) return BadRequest("Class deletion failed.");

            await _outputCache.EvictByTagAsync("classes", default);

            return NoContent();
        }
    }
}