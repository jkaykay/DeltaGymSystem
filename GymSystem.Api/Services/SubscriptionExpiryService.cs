using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Services;

public class SubscriptionExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpiryService> _logger;
    private readonly IOutputCacheStore _outputCache;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public SubscriptionExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpiryService> logger,
        IOutputCacheStore outputCache)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _outputCache = outputCache;
    }

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

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

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