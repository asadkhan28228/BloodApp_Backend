using BloodDonationAPI.BLL.DTOs.BloodBatch;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IBloodBatchService
    {
        Task<List<BloodBatchDto>> GetAllAsync();

        Task<BloodBatchDto?> GetByIdAsync(
            Guid id);

        Task<List<BloodBatchDto>> GetByBloodTypeAsync(
            string bloodType);

        Task<BloodBatchDto> AddAsync(
            CreateBloodBatchDto dto);

        Task<bool> UpdateAsync(
            Guid id,
            UpdateBloodBatchDto dto);

        Task<bool> DeleteAsync(
            Guid id);

        Task<int> MarkExpiredBatchesAsync();
    }
}