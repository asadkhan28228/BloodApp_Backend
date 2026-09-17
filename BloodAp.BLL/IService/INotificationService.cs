using BloodDonationAPI.BLL.DTOs.Notification;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId);

        Task<int> GetUnreadCountAsync(Guid userId);

        Task<bool> MarkAsReadAsync(
            Guid notificationId,
            Guid userId);

        Task<bool> MarkAllAsReadAsync(
            Guid userId);

        Task<bool> DeleteAsync(
            Guid notificationId,
            Guid userId);

        Task<List<CommunityNotificationDto>>
            GetCommunityNotificationsAsync();
    }
}