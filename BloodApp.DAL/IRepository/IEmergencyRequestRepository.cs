using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IEmergencyRequestRepository
    {
        Task<EmergencyRequest?> GetByIdAsync(
            Guid id);

        Task<List<EmergencyRequest>> GetAllAsync();

        Task<List<EmergencyRequest>> GetActiveAsync();

        Task<List<EmergencyRequest>> GetByReporterAsync(
            Guid reporterUid);

        Task<EmergencyRequest> AddAsync(
            EmergencyRequest emergency);

        Task UpdateAsync(
            EmergencyRequest emergency);

        Task DeleteAsync(
            EmergencyRequest emergency);
    }
}