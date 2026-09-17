using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IBloodBankSettingRepository
    {
        Task<BloodBankSetting?> GetByIdAsync(int id);

        Task<List<BloodBankSetting>> GetAllAsync();

        Task<BloodBankSetting> AddAsync(
            BloodBankSetting setting);

        Task UpdateAsync(
            BloodBankSetting setting);

        Task DeleteAsync(
            BloodBankSetting setting);
    }
}