using BloodDonationAPI.BLL.DTOs.Notification;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface ICommunityNotificationService
    {
        Task<List<CommunityNotificationDto>> GetAllAsync();

        Task<CommunityNotificationDto?> GetByIdAsync(Guid id);

        Task<List<CommunityNotificationDto>> GetByReporterAsync(
            Guid reporterUid);

        Task<CommunityNotificationDto> CreateAsync(
            CommunityNotificationDto dto);

        Task<bool> MarkAsReadAsync(Guid id);

        Task<bool> DeleteAsync(Guid id);
    }
}