using GymSystem.Api.Data;
using GymSystem.Api.Extensions;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Staff")]
public class BranchController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly IOutputCacheStore _outputCache;

    public BranchController(GymDbContext context, IOutputCacheStore outputCache)
    {
        _context = context;
        _outputCache = outputCache;
    }

    [HttpGet]
    [OutputCache(PolicyName = "branches")]
    public async Task<IActionResult> GetAll([FromQuery] BranchSearchRequest request)
    {
        var query = _context.Branches.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(b =>
                b.Address.ToLower().Contains(term) ||
                b.City.ToLower().Contains(term) ||
                b.Province.ToLower().Contains(term) ||
                b.PostCode.ToLower().Contains(term));
        }

        var descending = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = request.SortBy?.ToLower() switch
        {
            "address"  => descending ? query.OrderByDescending(b => b.Address)  : query.OrderBy(b => b.Address),
            "province" => descending ? query.OrderByDescending(b => b.Province) : query.OrderBy(b => b.Province),
            "postcode" => descending ? query.OrderByDescending(b => b.PostCode) : query.OrderBy(b => b.PostCode),
            "opendate" => descending ? query.OrderByDescending(b => b.OpenDate) : query.OrderBy(b => b.OpenDate),
            _          => descending ? query.OrderByDescending(b => b.City)     : query.OrderBy(b => b.City),
        };

        var result = await query
            .Select(b => new BranchDTO
            {
                BranchId = b.BranchId,
                Address = b.Address,
                City = b.City,
                Province = b.Province,
                PostCode = b.PostCode,
                OpenDate = b.OpenDate
            })
            .ToPagedResultAsync(request.Page, request.PageSize);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [OutputCache(PolicyName = "branches")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] AddBranchRequest request)
    {
        var branch = new Branch
        {
            Address = request.Address,
            City = request.City,
            Province = request.Province,
            PostCode = request.PostCode,
            OpenDate = request.OpenDate ?? DateTime.UtcNow
        };

        _context.Branches.Add(branch);
        var rowsAffected = await _context.SaveChangesAsync();

        if (rowsAffected == 0)
            return BadRequest("Failed to create branch.");

        await _outputCache.EvictByTagAsync("branches", default);

        return CreatedAtAction(nameof(Get), new { id = branch.BranchId }, new BranchDTO
        {
            BranchId = branch.BranchId,
            Address = branch.Address,
            City = branch.City,
            Province = branch.Province,
            PostCode = branch.PostCode,
            OpenDate = branch.OpenDate
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch is null) return NotFound();

        _context.Branches.Remove(branch);
        var rowsAffected = await _context.SaveChangesAsync();
        if (rowsAffected == 0)
            return BadRequest("Failed to delete branch.");

        await _outputCache.EvictByTagAsync("branches", default);

        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchRequest request)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch is null) return NotFound();

        if (request.Address is not null) branch.Address = request.Address;
        if (request.City is not null) branch.City = request.City;
        if (request.Province is not null) branch.Province = request.Province;
        if (request.PostCode is not null) branch.PostCode = request.PostCode;

        await _context.SaveChangesAsync();

        await _outputCache.EvictByTagAsync("branches", default);

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

    [HttpGet("total")]
    [OutputCache(PolicyName = "branches")]
    public async Task<IActionResult> GetTotalBranches()
    {
        var totalBranches = await _context.Branches.CountAsync();
        return Ok(new CountResponse { Count = totalBranches });
    }
}