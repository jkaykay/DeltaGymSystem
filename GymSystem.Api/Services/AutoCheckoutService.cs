// ============================================================
// AutoCheckoutService.cs — Background service that automatically
// checks out members who forgot to scan out. Runs every 60 minutes
// and closes any attendance sessions older than 4 hours.
// This prevents "stuck" check-ins from appearing indefinitely.
// ============================================================

using GymSystem.Api.Data;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Services;

public class AutoCheckoutService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;           // Creates DI scopes for database access
    private readonly ILogger<AutoCheckoutService> _logger;         // Writes log messages
    private readonly IOutputCacheStore _outputCache;               // Clears cached attendance responses
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(60);  // Run every 60 minutes
    private readonly TimeSpan _maxSessionDuration = TimeSpan.FromHours(4); // Max 4 hours per visit

    public AutoCheckoutService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutoCheckoutService> logger,
        IOutputCacheStore outputCache)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _outputCache = outputCache;
    }

    // Runs in a loop for the lifetime of the application.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoCheckoutService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessStaleCheckInsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stale check-ins.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    // Finds attendance records that have been "checked in" for longer
    // than the max duration and automatically checks them out.
    private async Task ProcessStaleCheckInsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GymDbContext>();

        var cutoff = DateTime.UtcNow - _maxSessionDuration;

        var staleRecords = await context.Attendances
            .Where(a => a.InFlag && a.CheckIn < cutoff)
            .ToListAsync(ct);

        if (staleRecords.Count == 0)
            return;

        var now = DateTime.UtcNow;
        foreach (var record in staleRecords)
        {
            record.CheckOut = now;
            record.InFlag = false;
        }

        await context.SaveChangesAsync(ct);
        await _outputCache.EvictByTagAsync("attendance", ct);

        _logger.LogInformation(
            "Auto-checked out {Count} stale attendance record(s) older than {Hours}h.",
            staleRecords.Count, _maxSessionDuration.TotalHours);
    }
}