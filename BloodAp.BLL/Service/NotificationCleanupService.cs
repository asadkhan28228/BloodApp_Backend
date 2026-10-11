using BloodDonationAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BloodDonationAPI.API.Services
{
    /// <summary>
    /// Deletes private and community notifications after the configured retention period.
    /// Runs while the ASP.NET Core backend is running.
    /// </summary>
    public sealed class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationCleanupService> _logger;
        private readonly TimeSpan _retention;
        private readonly TimeSpan _interval;

        public NotificationCleanupService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<NotificationCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var retentionMinutes = double.TryParse(
                configuration["NotificationCleanup:RetentionMinutes"], out var minutes)
                && minutes > 0 ? minutes : 5;

            var intervalSeconds = double.TryParse(
                configuration["NotificationCleanup:CheckIntervalSeconds"], out var seconds)
                && seconds > 0 ? seconds : 30;

            _retention = TimeSpan.FromMinutes(retentionMinutes);
            _interval = TimeSpan.FromSeconds(intervalSeconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Notification cleanup started. Retention: {Minutes} minutes. Interval: {Seconds} seconds.",
                _retention.TotalMinutes, _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var cutoff = DateTime.UtcNow.Subtract(_retention);

                    var privateCount = await db.Notifications
                        .Where(n => n.CreatedAt <= cutoff)
                        .ExecuteDeleteAsync(stoppingToken);

                    var communityCount = await db.CommunityNotifications
                        .Where(n => n.CreatedAt <= cutoff)
                        .ExecuteDeleteAsync(stoppingToken);

                    if (privateCount + communityCount > 0)
                    {
                        _logger.LogInformation(
                            "Deleted {PrivateCount} private and {CommunityCount} community notifications.",
                            privateCount, communityCount);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Notification cleanup failed; will retry.");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
