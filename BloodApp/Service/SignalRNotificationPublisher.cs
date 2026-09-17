using System.Net.Http.Json;
using System.Reflection;
using BloodDonationAPI.API.Hubs;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace BloodDonationAPI.API.Services
{
    public class SignalRNotificationPublisher : INotificationPublisher
    {
        private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserRepository _userRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SignalRNotificationPublisher> _logger;

        public SignalRNotificationPublisher(
            IHubContext<NotificationHub> hubContext,
            IUserRepository userRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<SignalRNotificationPublisher> logger)
        {
            _hubContext = hubContext;
            _userRepository = userRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task PublishCommunityNotificationAsync(object notification)
        {
            // App open: instant in-app update through SignalR.
            await _hubContext.Clients.All.SendAsync(
                "CommunityNotification",
                notification);

            // App background/killed: OS notification through Expo Push.
            var users = await _userRepository.GetAllAsync();
            var reporterId = TryGetGuid(notification, "ReporterUid")
                             ?? TryGetGuid(notification, "UserId");

            var tokens = users
                .Where(u => u.IsActive &&
                            u.Role == "user" &&
                            !string.IsNullOrWhiteSpace(u.PushToken) &&
                            (!reporterId.HasValue || u.Id != reporterId.Value))
                .Select(u => u.PushToken!)
                .Distinct()
                .ToList();

            await SendExpoPushAsync(tokens, notification);
        }

        public async Task PublishUserNotificationAsync(
            Guid userId,
            object notification)
        {
            // App open: private SignalR group.
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", notification);

            // App background/killed: push to that user's device.
            var user = await _userRepository.GetByIdAsync(userId);

            if (user is { IsActive: true } &&
                !string.IsNullOrWhiteSpace(user.PushToken))
            {
                await SendExpoPushAsync(
                    new[] { user.PushToken! },
                    notification);
            }
        }

        private async Task SendExpoPushAsync(
            IEnumerable<string> tokens,
            object notification)
        {
            var validTokens = tokens
                .Where(t => t.StartsWith("ExponentPushToken[") ||
                            t.StartsWith("ExpoPushToken["))
                .Distinct()
                .ToList();

            if (validTokens.Count == 0)
                return;

            var title = GetString(notification, "Title") ?? "Blood Care";
            var body = GetString(notification, "Message") ?? "You have a new notification.";
            var type = GetString(notification, "Type") ?? "general";
            var id = GetString(notification, "Id");
            var referenceId = GetString(notification, "ReferenceId")
                              ?? GetString(notification, "EmergencyId")
                              ?? GetString(notification, "EmergencyRequestId");

            var client = _httpClientFactory.CreateClient();

            foreach (var chunk in validTokens.Chunk(100))
            {
                var messages = chunk.Select(token => new
                {
                    to = token,
                    sound = "default",
                    title,
                    body,
                    priority = "high",
                    channelId = "default",
                    data = new
                    {
                        type,
                        notificationId = id,
                        referenceId
                    }
                }).ToArray();

                try
                {
                    using var response = await client.PostAsJsonAsync(
                        ExpoPushUrl,
                        messages);

                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            "Expo push failed with status {Status}: {Body}",
                            response.StatusCode,
                            responseBody);
                    }
                }
                catch (Exception ex)
                {
                    // A push delivery failure must not break the original API action.
                    _logger.LogError(ex, "Failed to send Expo push notification.");
                }
            }
        }

        private static string? GetString(object source, string propertyName)
        {
            var value = source.GetType()
                .GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?.GetValue(source);

            return value?.ToString();
        }

        private static Guid? TryGetGuid(object source, string propertyName)
        {
            var value = GetString(source, propertyName);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
