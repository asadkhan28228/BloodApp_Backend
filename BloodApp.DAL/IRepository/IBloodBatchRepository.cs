using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IBloodBatchRepository
    {
        Task<BloodBatch?> GetByIdAsync(Guid id);

        Task<List<BloodBatch>> GetAllAsync();

        Task<List<BloodBatch>> GetByBloodTypeAsync(
            string bloodType);

        Task<List<BloodBatch>> GetActiveBatchesAsync(
            string bloodType);

        Task<BloodBatch> AddAsync(
            BloodBatch batch);

        Task UpdateAsync(
            BloodBatch batch);

        Task DeleteAsync(
            BloodBatch batch);
    }
}