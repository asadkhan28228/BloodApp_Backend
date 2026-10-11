
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using BloodDonationAPI.API.Hubs;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace BloodDonationAPI.API.Services
{
    public class SignalRNotificationPublisher : INotificationPublisher
    {
        private const string ExpoPushUrl =
            "https://exp.host/--/api/v2/push/send";

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

        // ============================================
        // COMMUNITY NOTIFICATIONS
        // ============================================

        public async Task PublishCommunityNotificationAsync(
            object notification)
        {
            var users = await _userRepository.GetAllAsync();

            var reporterId =
                TryGetGuid(notification, "ReporterUid")
                ?? TryGetGuid(notification, "UserId");

            var type = (
                GetString(notification, "Type") ?? ""
            ).Trim().ToLowerInvariant();

            // Exclude the creator from ALL community notifications.
            // This includes:
            // - Blood requests
            // - New registrations
            // - Emergency community notifications
            //
            // All other active normal users will receive them.

            var recipients = users
                .Where(u =>
                    u.IsActive &&
                    string.Equals(
                        u.Role,
                        "user",
                        StringComparison.OrdinalIgnoreCase) &&
                    (
                        !reporterId.HasValue ||
                        u.Id != reporterId.Value
                    ))
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();

            // ========================================
            // REAL-TIME SIGNALR
            // ========================================

            var sendTasks = recipients.Select(user =>
                _hubContext.Clients
                    .Group($"user_{user.Id}")
                    .SendAsync(
                        "CommunityNotification",
                        notification));

            await Task.WhenAll(sendTasks);

            _logger.LogInformation(
                "Community notification {NotificationId} " +
                "of type {NotificationType} sent to " +
                "{RecipientCount} active users. " +
                "Creator excluded: {ReporterId}",
                GetString(notification, "Id"),
                type,
                recipients.Count,
                reporterId);

            // ========================================
            // EXPO PUSH NOTIFICATIONS
            // ========================================

            var tokens = recipients
                .Where(u =>
                    !string.IsNullOrWhiteSpace(u.PushToken))
                .Select(u => u.PushToken!)
                .Distinct()
                .ToList();

            await SendExpoPushAsync(tokens, notification);
        }

        // ============================================
        // PRIVATE USER NOTIFICATIONS
        // ============================================

        public async Task PublishUserNotificationAsync(
            Guid userId,
            object notification)
        {
            // Private notifications are sent only
            // to their intended recipient.

            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync(
                    "ReceiveNotification",
                    notification);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user is { IsActive: true } &&
                !string.IsNullOrWhiteSpace(user.PushToken))
            {
                await SendExpoPushAsync(
                    new[] { user.PushToken! },
                    notification);
            }
        }

        // ============================================
        // EXPO PUSH DELIVERY
        // ============================================

        private async Task SendExpoPushAsync(
            IEnumerable<string> tokens,
            object notification)
        {
            var validTokens = tokens
                .Where(t =>
                    !string.IsNullOrWhiteSpace(t) &&
                    (
                        t.StartsWith("ExponentPushToken[") ||
                        t.StartsWith("ExpoPushToken[")
                    ))
                .Distinct()
                .ToList();

            if (validTokens.Count == 0)
                return;

            var title =
                GetString(notification, "Title")
                ?? "Blood Care";

            var body =
                GetString(notification, "Message")
                ?? "You have a new notification.";

            var type =
                GetString(notification, "Type")
                ?? "general";

            var id = GetString(notification, "Id");

            var referenceId =
                GetString(notification, "ReferenceId")
                ?? GetString(notification, "EmergencyId")
                ?? GetString(notification, "EmergencyRequestId")
                ?? GetString(notification, "BloodRequestId");

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
                    using var response =
                        await client.PostAsJsonAsync(
                            ExpoPushUrl,
                            messages);

                    var responseBody =
                        await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            "Expo push failed. " +
                            "Status: {Status}, Body: {Body}",
                            response.StatusCode,
                            responseBody);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Expo push request accepted " +
                            "for {Count} device tokens.",
                            messages.Length);
                    }
                }
                catch (Exception ex)
                {
                    // Push failure must not break
                    // registration or blood request creation.
                    _logger.LogError(
                        ex,
                        "Failed to send Expo push notification.");
                }
            }
        }

        // ============================================
        // HELPER: GET STRING PROPERTY
        // ============================================

        private static string? GetString(
            object source,
            string propertyName)
        {
            var value = source.GetType()
                .GetProperty(
                    propertyName,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase)
                ?.GetValue(source);

            return value?.ToString();
        }

        // ============================================
        // HELPER: GET GUID PROPERTY
        // ============================================

        private static Guid? TryGetGuid(
            object source,
            string propertyName)
        {
            var value = GetString(source, propertyName);

            return Guid.TryParse(
                value,
                out var id)
                ? id
                : null;
        }
    }
}
