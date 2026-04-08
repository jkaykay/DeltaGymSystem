// ============================================================
// SubscriptionExpiryService.cs — Background service that runs
// automatically every hour to manage subscription lifecycles.
// It performs three tasks:
//   1. Marks active subscriptions as "Expired" if their end date passed.
//   2. Promotes "Queued" subscriptions to "Active" when their start date arrives.
//   3. Deactivates members who have no remaining active subscriptions.
// ============================================================

using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Services;

// BackgroundService is a built-in ASP.NET Core class for long-running tasks.
// It starts when the app starts and runs until the app shuts down.
public class SubscriptionExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;              // Creates DI scopes for database access
    private readonly ILogger<SubscriptionExpiryService> _logger;     // Writes log messages
    private readonly IOutputCacheStore _outputCache;                  // Clears cached API responses
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // How often to run

    public SubscriptionExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpiryService> logger,
        IOutputCacheStore outputCache)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _outputCache = outputCache;
    }

    // This method runs in a loop for the lifetime of the application.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SubscriptionExpiryService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSubscriptionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscriptions.");
            }

            // Wait before checking again
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    // Core logic: expire old subs, promote queued subs, deactivate members.
    private async Task ProcessSubscriptionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var now = DateTime.UtcNow;

        // 1. Expire active subscriptions that have passed their end date
        var expiredSubs = await context.Subscriptions
            .Where(s => s.State == SubscriptionState.Active && s.EndDate < now)
            .ToListAsync(ct);

        foreach (var sub in expiredSubs)
            sub.State = SubscriptionState.Expired;

        // 2. Promote queued subscriptions whose start date has arrived
        var readySubs = await context.Subscriptions
            .Where(s => s.State == SubscriptionState.Queued && s.StartDate <= now)
            .ToListAsync(ct);

        foreach (var sub in readySubs)
            sub.State = SubscriptionState.Active;

        await context.SaveChangesAsync(ct);

        if (expiredSubs.Count > 0 || readySubs.Count > 0)
        {
            await _outputCache.EvictByTagAsync("subscriptions", ct);
            _logger.LogInformation("Processed {Expired} expired, {Promoted} promoted subscriptions.",
                expiredSubs.Count, readySubs.Count);
        }

        // 3. Deactivate members who have no Active or Queued subscriptions left
        var affectedUserIds = expiredSubs.Select(s => s.UserId).ToHashSet();
        var membersDeactivated = 0;

        foreach (var userId in affectedUserIds)
        {
            var hasActiveSub = await context.Subscriptions
                .AnyAsync(s => s.UserId == userId
                            && (s.State == SubscriptionState.Active || s.State == SubscriptionState.Queued), ct);

            if (!hasActiveSub)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user is not null && user.Active)
                {
                    user.Active = false;
                    await userManager.UpdateAsync(user);
                    membersDeactivated++;
                    _logger.LogInformation("Member {UserId} deactivated — no active subscriptions remain.", userId);
                }
            }
        }

        if (membersDeactivated > 0)
            await _outputCache.EvictByTagAsync("members", ct);
    }
}