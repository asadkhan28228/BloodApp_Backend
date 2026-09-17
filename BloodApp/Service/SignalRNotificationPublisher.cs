using BloodDonationAPI.API.Hubs;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BloodDonationAPI.API.Services
{
    public class SignalRNotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationPublisher(
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishCommunityNotificationAsync(
            object notification)
        {
            await _hubContext.Clients.All
                .SendAsync(
                    "CommunityNotification",
                    notification);
        }
    }
}