using BloodDonationAPI.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BloodDonationAPI.BLL.Services
{
    public class BloodRequestExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BloodRequestExpiryService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
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

                    foreach (var request in requests)
                    {
                        if (request.Status == "Pending" &&
                            request.ExpireAt.HasValue &&
                            request.ExpireAt.Value <= now)
                        {
                            await repository.DeleteAsync(request);
                        }
                    }
                }
                catch
                {
                    // Keep background service running
                }

                // Check every 5 minutes
                //await Task.Delay(TimeSpan.FromMinutes(5),stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}