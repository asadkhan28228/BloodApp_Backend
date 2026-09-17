using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface INotificationRepository
    {
        // ==========================================
        // PRIVATE NOTIFICATIONS
        // ==========================================

        Task<Notification?> GetByIdAsync(Guid id);

        Task<List<Notification>> GetUserNotificationsAsync(
            Guid userId);

        Task<int> GetUnreadCountAsync(
            Guid userId);

        Task<Notification> AddAsync(
            Notification notification);

        Task UpdateAsync(
            Notification notification);

        Task DeleteAsync(
            Notification notification);


        // ==========================================
        // COMMUNITY NOTIFICATIONS
        // ==========================================

        Task<List<CommunityNotification>>
            GetCommunityNotificationsAsync();

        Task<CommunityNotification> AddCommunityAsync(
            CommunityNotification notification);

        Task DeleteCommunityAsync(
            CommunityNotification notification);
    }
}