using BloodDonationAPI.BLL.DTOs.BloodBank;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IBloodBankSettingService
    {
        Task<List<BloodBankSettingDto>> GetAllAsync();

        Task<BloodBankSettingDto?> GetByIdAsync(int id);

        Task<BloodBankSettingDto> AddAsync(
            UpdateBloodBankSettingDto dto);

        Task<BloodBankSettingDto?> UpdateAsync(
            int id,
            UpdateBloodBankSettingDto dto);

        Task<bool> DeleteAsync(int id);
    }
}