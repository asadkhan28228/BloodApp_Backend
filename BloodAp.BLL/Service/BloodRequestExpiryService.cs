using BloodDonationAPI.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BloodDonationAPI.BLL.Services
{
    public class BloodRequestExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BloodRequestExpiryService> _logger;

        public BloodRequestExpiryService(
            IServiceScopeFactory scopeFactory,
            ILogger<BloodRequestExpiryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Blood Request Expiry Service started.");

            // Backend start hote hi expired requests check karega
            await DeleteExpiredRequestsAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Har 1 minute baad check
                    await Task.Delay(
                        TimeSpan.FromMinutes(1),
                        stoppingToken);

                    await DeleteExpiredRequestsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Backend normally stop hone par
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Blood request expiry service error.");
                }
            }

            _logger.LogInformation(
                "Blood Request Expiry Service stopped.");
        }

        private async Task DeleteExpiredRequestsAsync(
            CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<IBloodRequestRepository>();

                var requests =
                    await repository.GetAllAsync();

                var now = DateTime.UtcNow;

                var expiredRequests = requests
                    .Where(request =>
                        request.ExpireAt.HasValue &&
                        request.ExpireAt.Value <= now)
                    .ToList();

                foreach (var request in expiredRequests)
                {
                    await repository.DeleteAsync(request);

                    _logger.LogInformation(
                        "Expired blood request deleted: {RequestId}",
                        request.Id);
                }

                if (expiredRequests.Count > 0)
                {
                    _logger.LogInformation(
                        "{Count} expired blood request(s) deleted.",
                        expiredRequests.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete expired blood requests.");
            }
        }
    }
}