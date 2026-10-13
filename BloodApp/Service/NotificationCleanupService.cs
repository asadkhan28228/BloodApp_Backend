
using BloodDonationAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.API.Services
{
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationCleanupService> _logger;

        public NotificationCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            // Run immediately, then every hour.
            using var timer = new PeriodicTimer(
                TimeSpan.FromHours(1));

            do
            {
                try
                {
                    await CleanupAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Notification, blood request and emergency cleanup failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task CleanupAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var cutoff = DateTime.UtcNow.AddHours(-24);

            // ==========================================
            // 1. DELETE OLD PRIVATE NOTIFICATIONS
            // ==========================================

            var privateDeleted = await db.Notifications
                .Where(x => x.CreatedAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            // ==========================================
            // 2. DELETE OLD COMMUNITY NOTIFICATIONS
            // ==========================================

            var communityDeleted = await db.CommunityNotifications
                .Where(x => x.CreatedAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            // ==========================================
            // 3. FIND EXPIRED EMERGENCY REQUESTS
            // ==========================================

            var expiredEmergencyIds = await db.EmergencyRequests
                .Where(x => x.CreatedAt < cutoff)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var emergencyDeleted = 0;

            if (expiredEmergencyIds.Count > 0)
            {
                // Delete related emergency donor responses
                await db.EmergencyResponses
                    .Where(x => expiredEmergencyIds
                        .Contains(x.EmergencyRequestId))
                    .ExecuteDeleteAsync(cancellationToken);

                // Unlink blood requests from expired emergencies
                await db.BloodRequests
                    .Where(x =>
                        x.EmergencyId.HasValue &&
                        expiredEmergencyIds.Contains(
                            x.EmergencyId.Value))
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            x => x.EmergencyId,
                            (Guid?)null),
                        cancellationToken);

                // Delete expired emergencies
                emergencyDeleted = await db.EmergencyRequests
                    .Where(x => expiredEmergencyIds.Contains(x.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // ==========================================
            // 4. FIND EXPIRED NORMAL BLOOD REQUESTS
            // ==========================================

            var expiredBloodRequestIds = await db.BloodRequests
                .Where(x =>
                    x.CreatedAt < cutoff &&
                    x.RequestType == "BloodRequest")
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var bloodRequestsDeleted = 0;

            if (expiredBloodRequestIds.Count > 0)
            {
                // Delete related donor responses first
                await db.BloodRequestResponses
                    .Where(x => expiredBloodRequestIds
                        .Contains(x.BloodRequestId))
                    .ExecuteDeleteAsync(cancellationToken);

                // Delete expired normal blood requests
                bloodRequestsDeleted = await db.BloodRequests
                    .Where(x => expiredBloodRequestIds.Contains(x.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // ==========================================
            // 5. LOG CLEANUP RESULT
            // ==========================================

            _logger.LogInformation(
                "Cleanup completed: {PrivateCount} private, " +
                "{CommunityCount} community, " +
                "{EmergencyCount} emergencies, " +
                "{BloodCount} normal blood requests deleted.",
                privateDeleted,
                communityDeleted,
                emergencyDeleted,
                bloodRequestsDeleted);
        }
    }
}
