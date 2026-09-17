using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface ICommunityNotificationRepository
    {
        Task<CommunityNotification?> GetByIdAsync(Guid id);

        Task<List<CommunityNotification>> GetAllAsync();

        Task<List<CommunityNotification>> GetByReporterAsync(
            Guid reporterUid);

        Task<List<CommunityNotification>> GetByEmergencyRequestAsync(
            Guid emergencyRequestId);

        Task<CommunityNotification> AddAsync(
            CommunityNotification notification);

        Task UpdateAsync(
            CommunityNotification notification);

        Task DeleteAsync(
            CommunityNotification notification);
    }
}