using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BloodDonationAPI.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userId, out var id))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{id}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userId, out var id))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{id}");
            await base.OnDisconnectedAsync(exception);
        }

        // Compatibility method: users may only join their OWN private group.
        public async Task JoinUserGroup(string userId)
        {
            var authenticatedId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var requestedId) ||
                !Guid.TryParse(authenticatedId, out var currentId) ||
                requestedId != currentId)
                throw new HubException("Cannot join another user's group.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{currentId}");
        }

        public Task SendTestNotification(string message) =>
            Clients.Caller.SendAsync("ReceiveNotification", new
            {
                type = "test",
                title = "Test Notification",
                message,
                createdAt = DateTime.UtcNow
            });
    }
}
