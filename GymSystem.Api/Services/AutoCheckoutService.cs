using GymSystem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Services;

public class AutoCheckoutService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoCheckoutService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(60);
    private readonly TimeSpan _maxSessionDuration = TimeSpan.FromHours(4);

    public AutoCheckoutService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutoCheckoutService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

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

        _logger.LogInformation(
            "Auto-checked out {Count} stale attendance record(s) older than {Hours}h.",
            staleRecords.Count, _maxSessionDuration.TotalHours);
    }
}