// ============================================================
// QRCodeController.cs — QR code generation and scanning endpoints.
// Members get a time-limited QR code; staff scan it to check
// the member in or out. The QR token is HMAC-signed to prevent
// tampering and cached to avoid regenerating on every request.
// ============================================================

using System.Security.Claims;
using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Api.Services;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QRCoder;

namespace GymSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QRCodeController : ControllerBase
{
    private readonly IQRTokenService _qrTokenService;          // Generates/validates QR tokens
    private readonly GymDbContext _context;                     // Database context
    private readonly UserManager<ApplicationUser> _userManager; // Manages user lookups
    private readonly IMemoryCache _memoryCache;                // Caches generated QR images
    private readonly IOutputCacheStore _outputCache;           // Invalidates attendance cache

    public QRCodeController(
        IQRTokenService qrTokenService,
        GymDbContext context,
        UserManager<ApplicationUser> userManager,
        IMemoryCache memoryCache,
        IOutputCacheStore outputCache)
    {
        _qrTokenService = qrTokenService;
        _context = context;
        _userManager = userManager;
        _memoryCache = memoryCache;
        _outputCache = outputCache;
    }

    // GET api/qrcode/generate/{memberId} — Generate a QR code for a member.
    // Members can only generate their own; Staff/Admin can generate for anyone.
    // The QR code is cached until its token expires.
    [Authorize]
    [HttpGet("generate/{memberId}")]
    [EnableRateLimiting("qr")]
    public async Task<IActionResult> Generate(string memberId)
    {
        // Members can only generate their own QR code
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isStaffOrAdmin = User.IsInRole("Staff") || User.IsInRole("Admin");

        if (!isStaffOrAdmin && callerId != memberId)
            return Forbid();

        // Inactive / unsubscribed members must not receive a QR code
        var user = await _userManager.FindByIdAsync(memberId);
        if (user is null)
            return NotFound(new { message = $"No member found with ID '{memberId}'." });

        if (!user.Active)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = $"{user.FirstName} {user.LastName} is not an active member." });

        var cacheKey = $"qr:{memberId}";

        // Return the cached PNG if the token is still within its validity window
        if (_memoryCache.TryGetValue(cacheKey, out QrCacheEntry? cached) && cached!.ExpiresAt > DateTime.UtcNow)
        {
            return Ok(new QRCodeResponse(cached.QrBase64, cached.ExpiresAt));
        }

        // Cache miss — generate a new token and render the PNG
        var result = _qrTokenService.GenerateToken(memberId);
        var qrBase64 = RenderQrCodeBase64(result.Token);

        _memoryCache.Set(cacheKey, new QrCacheEntry(qrBase64, result.ExpiresAt),
            new MemoryCacheEntryOptions
            {
                // Entry expires exactly when the QR token itself expires
                AbsoluteExpiration = new DateTimeOffset(result.ExpiresAt, TimeSpan.Zero)
            });

        return Ok(new QRCodeResponse(qrBase64, result.ExpiresAt));
    }

    // POST api/qrcode/scan — Staff scans a member's QR code.
    // Automatically checks them in (if not already in) or out (if already in).
    /// Staff scans a member's QR code — automatically checks them in or out.
    [Authorize(Roles = "Staff,Admin")]
    [HttpPost("scan")]
    [EnableRateLimiting("qr")]
    public async Task<IActionResult> Scan([FromBody] ScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { message = "Token is required." });

        var payload = _qrTokenService.ValidateToken(request.Token);

        if (payload is null)
            return Unauthorized(new { message = "QR code is invalid or has expired." });

        var user = await _userManager.FindByIdAsync(payload.MemberId);
        if (user is null)
            return NotFound(new { message = $"No member found with ID '{payload.MemberId}'." });

        if (!user.Active)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = $"{user.FirstName} {user.LastName} is not an active member." });

        var memberName = $"{user.FirstName} {user.LastName}";

        // Check if member has an open session → check out; otherwise → check in
        var openSession = await _context.Attendances
            .FirstOrDefaultAsync(a => a.UserId == payload.MemberId && a.InFlag);

        if (openSession is not null)
        {
            // Check out
            openSession.CheckOut = DateTime.UtcNow;
            openSession.InFlag = false;
            await _context.SaveChangesAsync();

            await _outputCache.EvictByTagAsync("attendance", default);

            return Ok(new ScanResponse("checkout", payload.MemberId, memberName,
                CheckIn: null, CheckOut: openSession.CheckOut));
        }

        var attendance = new Attendance
        {
            CheckIn = DateTime.UtcNow,
            CheckOut = default,
            InFlag = true,
            UserId = payload.MemberId,
            User = user
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        await _outputCache.EvictByTagAsync("attendance", default);

        return Ok(new ScanResponse("checkin", payload.MemberId, memberName,
            CheckIn: attendance.CheckIn, CheckOut: null));
    }

    // Renders the token string into a QR code PNG image and returns it as Base64.
    private static string RenderQrCodeBase64(string content)
    {
        using var generator = new QRCodeGenerator();
        var qrData = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(pixelsPerModule: 20);
        return Convert.ToBase64String(pngBytes);
    }

    // Holds the rendered PNG and expiry so we can skip re-generation on cache hits
    private sealed record QrCacheEntry(string QrBase64, DateTime ExpiresAt);
}