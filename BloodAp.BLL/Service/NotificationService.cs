using BloodDonationAPI.BLL.DTOs.Notification;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // =========================
        // PRIVATE NOTIFICATIONS
        // =========================

        public async Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId)
        {
            var notifications =
                await _notificationRepository.GetUserNotificationsAsync(userId);

            return notifications.Select(MapToDto).ToList();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification =
                await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null)
                return false;

            if (notification.UserId != userId)
                return false;

            if (!notification.Unread)
                return true;

            notification.Unread = false;
            notification.ReadAt = DateTime.UtcNow;

            await _notificationRepository.UpdateAsync(notification);

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var notifications =
                await _notificationRepository.GetUserNotificationsAsync(userId);

            foreach (var notification in notifications)
            {
                if (notification.Unread)
                {
                    notification.Unread = false;
                    notification.ReadAt = DateTime.UtcNow;

                    await _notificationRepository.UpdateAsync(notification);
                }
            }

            return true;
        }

        public async Task<bool> DeleteAsync(Guid notificationId, Guid userId)
        {
            var notification =
                await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null)
                return false;

            if (notification.UserId != userId)
                return false;

            await _notificationRepository.DeleteAsync(notification);

            return true;
        }

        // =========================
        // COMMUNITY NOTIFICATIONS
        // =========================

        public async Task<List<CommunityNotificationDto>>
            GetCommunityNotificationsAsync()
        {
            var notifications =
                await _notificationRepository.GetCommunityNotificationsAsync();

            return notifications.Select(MapCommunityToDto).ToList();
        }

        // =========================
        // MAPPING
        // =========================

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                Icon = notification.Icon,
                Color = notification.Color,
                Unread = notification.Unread,
                TargetScreen = notification.TargetScreen,
                ReferenceId = notification.ReferenceId,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }

        private static CommunityNotificationDto MapCommunityToDto(
            CommunityNotification notification)
        {
            return new CommunityNotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                Unread = notification.Unread,
                Icon = notification.Icon,
                Color = notification.Color,
                ReporterUid = notification.ReporterUid,
                ReporterName = notification.ReporterName,
                BloodRequestId = notification.BloodRequestId,
                EmergencyRequestId = notification.EmergencyRequestId,
                Status = notification.Status,
                Category = notification.Category,
                Priority = notification.Priority,
                BloodType = notification.BloodType,
                UnitsNeeded = notification.UnitsNeeded,
                BloodSource = notification.BloodSource,
                ReservedUnits = notification.ReservedUnits,
                Location = notification.Location,
                Latitude = notification.Latitude,
                Longitude = notification.Longitude,
                PhoneNumber = notification.PhoneNumber,
                Details = notification.Details,
                ExpireAt = notification.ExpireAt,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt
            };
        }
    }
}