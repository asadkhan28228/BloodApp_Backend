using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class NotificationRepository
        : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdAsync(
            Guid id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Notification>>
            GetUserNotificationsAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(
            Guid userId)
        {
            return await _context.Notifications
                .CountAsync(x =>
                    x.UserId == userId &&
                    x.Unread);
        }

        public async Task<Notification> AddAsync(
            Notification notification)
        {
            await _context.Notifications
                .AddAsync(notification);

            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task UpdateAsync(
            Notification notification)
        {
            _context.Notifications.Update(notification);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            Notification notification)
        {
            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();
        }

        public async Task<List<CommunityNotification>>
            GetCommunityNotificationsAsync()
        {
            return await _context.CommunityNotifications
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<CommunityNotification>
            AddCommunityAsync(
                CommunityNotification notification)
        {
            await _context.CommunityNotifications
                .AddAsync(notification);

            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task DeleteCommunityAsync(
            CommunityNotification notification)
        {
            _context.CommunityNotifications
                .Remove(notification);

            await _context.SaveChangesAsync();
        }
    }
}