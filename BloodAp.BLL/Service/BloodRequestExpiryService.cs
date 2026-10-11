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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Blood Request Expiry Service started.");

            // Check on startup and then every minute.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DeleteExpiredRequestsAsync();
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Blood request expiry service error.");
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }

            _logger.LogInformation("Blood Request Expiry Service stopped.");
        }

        private async Task DeleteExpiredRequestsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider
                .GetRequiredService<IBloodRequestRepository>();

            var deletedCount = await repository.DeleteExpiredAsync(DateTime.UtcNow);
            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Automatically deleted {Count} unanswered blood request(s) older than 24 hours.",
                    deletedCount);
            }
        }
    }
}
