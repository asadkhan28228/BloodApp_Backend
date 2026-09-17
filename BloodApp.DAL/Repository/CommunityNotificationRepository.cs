using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class CommunityNotificationRepository
        : ICommunityNotificationRepository
    {
        private readonly AppDbContext _context;

        public CommunityNotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CommunityNotification?> GetByIdAsync(Guid id)
        {
            return await _context.CommunityNotifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<CommunityNotification>> GetAllAsync()
        {
            return await _context.CommunityNotifications
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CommunityNotification>> GetByReporterAsync(
            Guid reporterUid)
        {
            return await _context.CommunityNotifications
                .AsNoTracking()
                .Where(x => x.ReporterUid == reporterUid)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // =========================================================
        // GET NOTIFICATIONS BY EMERGENCY
        // =========================================================

        public async Task<List<CommunityNotification>>
            GetByEmergencyRequestAsync(Guid emergencyRequestId)
        {
            return await _context.CommunityNotifications
                .Where(x => x.EmergencyRequestId == emergencyRequestId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<CommunityNotification> AddAsync(
            CommunityNotification notification)
        {
            await _context.CommunityNotifications.AddAsync(notification);

            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task UpdateAsync(
            CommunityNotification notification)
        {
            _context.CommunityNotifications.Update(notification);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            CommunityNotification notification)
        {
            _context.CommunityNotifications.Remove(notification);

            await _context.SaveChangesAsync();
        }
    }
}