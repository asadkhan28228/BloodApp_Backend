using BloodDonationAPI.BLL.DTOs.BloodInventory;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IBloodInventoryService
    {
        Task<List<BloodInventoryDto>> GetAllAsync();

        Task<BloodInventoryDto?> GetByBloodTypeAsync(
            string bloodType);

        Task<bool> IncreaseUnitsAsync(
            string bloodType,
            int units);

        Task<bool> DecreaseUnitsAsync(
            string bloodType,
            int units);

        Task<bool> UpdateAsync(
            UpdateBloodInventoryDto dto);
    }
}