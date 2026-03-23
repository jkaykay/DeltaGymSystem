using System.Security.Claims;
using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Api.Services;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace GymSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QRCodeController : ControllerBase
{
    private readonly IQRTokenService _qrTokenService;
    private readonly GymDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public QRCodeController(
        IQRTokenService qrTokenService,
        GymDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _qrTokenService = qrTokenService;
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    [HttpGet("generate/{memberId}")]
    public IActionResult Generate(string memberId)
    {
        // Members can only generate their own QR code
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isStaffOrAdmin = User.IsInRole("Staff") || User.IsInRole("Admin");

        if (!isStaffOrAdmin && callerId != memberId)
            return Forbid();

        var result = _qrTokenService.GenerateToken(memberId);
        var qrBase64 = RenderQrCodeBase64(result.Token);

        return Ok(new
        {
            qrCodeBase64 = qrBase64,
            expiresAt = result.ExpiresAt,
            note = "Display as: <img src=\"data:image/png;base64,{qrCodeBase64}\" />"
        });
    }

    /// Staff scans a member's QR code — automatically checks them in or out.
    [Authorize(Roles = "Staff,Admin")]
    [HttpPost("scan")]
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

        // Check if member has an open session → check out; otherwise → check in
        var openSession = await _context.Attendances
            .FirstOrDefaultAsync(a => a.UserId == payload.MemberId && a.InFlag);

        if (openSession is not null)
        {
            // Check out
            openSession.CheckOut = DateTime.UtcNow;
            openSession.InFlag = false;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                action = "checkout",
                memberId = payload.MemberId,
                memberName = $"{user.FirstName} {user.LastName}",
                checkOut = openSession.CheckOut
            });
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

        return Ok(new
        {
            action = "checkin",
            memberId = payload.MemberId,
            memberName = $"{user.FirstName} {user.LastName}",
            checkIn = attendance.CheckIn
        });
    }

    private static string RenderQrCodeBase64(string content)
    {
        using var generator = new QRCodeGenerator();
        var qrData = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(pixelsPerModule: 20);
        return Convert.ToBase64String(pngBytes);
    }
}