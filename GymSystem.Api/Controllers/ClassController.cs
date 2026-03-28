using GymSystem.Api.Data;
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

        [HttpGet]
        [OutputCache(PolicyName = "classes")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.Classes
                .Select(c => new ClassDTO
                {
                    ClassId = c.ClassId,
                    Subject = c.Subject,
                    UserId = c.UserId,
                    TrainerName = $"{c.User.FirstName} {c.User.LastName}",
                    SessionCount = c.Sessions.Count
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [OutputCache(PolicyName = "classes")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _context.Classes
                .Where(c => c.ClassId == id)
                .Select(c => new ClassDTO
                {
                    ClassId = c.ClassId,
                    Subject = c.Subject,
                    UserId = c.UserId,
                    TrainerName = $"{c.User.FirstName} {c.User.LastName}",
                    SessionCount = c.Sessions.Count
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassRequest request)
        {
            var classData = await _context.Classes
                .Include(c => c.User)
                .Where(c => c.ClassId == id)
                .Select(c => new
                {
                    ClassObject = c,
                    SessionCount = c.Sessions.Count
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
                SessionCount = classData.SessionCount
            });
        }

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