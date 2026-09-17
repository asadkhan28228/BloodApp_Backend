using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class BloodBankSettingRepository
        : IBloodBankSettingRepository
    {
        private readonly AppDbContext _context;

        public BloodBankSettingRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<BloodBankSetting?> GetByIdAsync(int id)
        {
            return await _context.BloodBankSettings
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<BloodBankSetting>> GetAllAsync()
        {
            return await _context.BloodBankSettings
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<BloodBankSetting> AddAsync(
            BloodBankSetting setting)
        {
            await _context.BloodBankSettings.AddAsync(setting);

            await _context.SaveChangesAsync();

            return setting;
        }

        public async Task UpdateAsync(
            BloodBankSetting setting)
        {
            _context.BloodBankSettings.Update(setting);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            BloodBankSetting setting)
        {
            _context.BloodBankSettings.Remove(setting);

            await _context.SaveChangesAsync();
        }
    }
}