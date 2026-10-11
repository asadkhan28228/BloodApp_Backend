using BloodDonationAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BloodDonationAPI.API.Services
{
    /// <summary>
    /// Removes private and community notifications older than the configured retention.
    /// Runs only while the ASP.NET Core backend is running.
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

            var retentionHours = int.TryParse(
                configuration["NotificationCleanup:RetentionHours"], out var hours)
                && hours > 0 ? hours : 48;
            var intervalMinutes = int.TryParse(
                configuration["NotificationCleanup:CheckIntervalMinutes"], out var minutes)
                && minutes > 0 ? minutes : 1;

            _retention = TimeSpan.FromHours(retentionHours);
            _interval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification cleanup started. Retention: {Hours} hours.",
                _retention.TotalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var cutoff = DateTime.UtcNow.Subtract(_retention);

                    // ExecuteDeleteAsync deletes directly in SQL Server, without loading rows.
                    var privateCount = await db.Notifications
                        .Where(n => n.CreatedAt < cutoff)
                        .ExecuteDeleteAsync(stoppingToken);
                    var communityCount = await db.CommunityNotifications
                        .Where(n => n.CreatedAt < cutoff)
                        .ExecuteDeleteAsync(stoppingToken);

                    if (privateCount + communityCount > 0)
                        _logger.LogInformation(
                            "Deleted {PrivateCount} private and {CommunityCount} community notifications.",
                            privateCount, communityCount);
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
