using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IBloodRequestRepository
    {
        Task<BloodRequest?> GetByIdAsync(Guid id);

        Task<List<BloodRequest>> GetAllAsync();

        Task<List<BloodRequest>> GetByReporterAsync(Guid reporterUid);

        Task<List<BloodRequest>> GetPendingAsync();

        Task<BloodRequest?> GetByEmergencyIdAsync(Guid emergencyId);

        Task<BloodRequest> AddAsync(BloodRequest request);

        Task UpdateAsync(BloodRequest request);

        Task DeleteAsync(BloodRequest request);
    }
}