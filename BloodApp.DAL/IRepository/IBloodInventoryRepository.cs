using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IBloodInventoryRepository
    {
        Task<List<BloodInventory>> GetAllAsync();

        Task<BloodInventory?> GetByBloodTypeAsync(
            string bloodType);

        Task<BloodInventory> GetOrCreateAsync(
            string bloodType);

        Task<BloodInventory> AddAsync(
            BloodInventory inventory);

        Task IncreaseUnitsAsync(
            string bloodType,
            int units);

        Task<bool> DecreaseUnitsAsync(
            string bloodType,
            int units);

        Task<bool> ReserveUnitsAsync(
            string bloodType,
            int units);

        Task ReleaseUnitsAsync(
            string bloodType,
            int units);

        Task UpdateAsync(
            BloodInventory inventory);
    }
}