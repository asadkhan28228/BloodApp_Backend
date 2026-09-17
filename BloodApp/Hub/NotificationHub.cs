using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BloodDonationAPI.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId =
                Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"user_{userId}"
                );
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(
            Exception? exception)
        {
            var userId =
                Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    $"user_{userId}"
                );
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user_{userId}"
            );
        }

        public async Task SendTestNotification(string message)
        {
            await Clients.Caller.SendAsync(
                "ReceiveNotification",
                new
                {
                    type = "test",
                    title = "Test Notification",
                    message = message,
                    createdAt = DateTime.UtcNow
                }
            );
        }
    }
}