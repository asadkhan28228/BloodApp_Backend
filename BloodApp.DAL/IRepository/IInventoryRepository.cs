using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IInventoryRepository
    {
        // ==========================================
        // INVENTORY
        // ==========================================

        Task<BloodInventory?> GetInventoryAsync(
            string bloodType);

        Task<List<BloodInventory>> GetAllInventoryAsync();

        Task<BloodInventory> AddInventoryAsync(
            BloodInventory inventory);

        Task UpdateInventoryAsync(
            BloodInventory inventory);


        // ==========================================
        // BLOOD BATCHES
        // ==========================================

        Task<BloodBatch?> GetBatchAsync(Guid id);

        Task<List<BloodBatch>> GetAllBatchesAsync();

        Task<List<BloodBatch>> GetBatchesByBloodTypeAsync(
            string bloodType);

        Task<BloodBatch> AddBatchAsync(
            BloodBatch batch);

        Task UpdateBatchAsync(
            BloodBatch batch);

        Task DeleteBatchAsync(
            BloodBatch batch);
    }
}