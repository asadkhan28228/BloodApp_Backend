using BloodDonationAPI.BLL.DTOs.BloodBank;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class BloodBankSettingService
        : IBloodBankSettingService
    {
        private readonly IBloodBankSettingRepository _repository;

        public BloodBankSettingService(
            IBloodBankSettingRepository repository)
        {
            _repository = repository;
        }

        // ==========================================
        // GET ALL BLOOD BANKS
        // ==========================================

        public async Task<List<BloodBankSettingDto>> GetAllAsync()
        {
            var bloodBanks = await _repository.GetAllAsync();

            return bloodBanks
                .Select(MapToDto)
                .ToList();
        }

        // ==========================================
        // GET BLOOD BANK BY ID
        // ==========================================

        public async Task<BloodBankSettingDto?> GetByIdAsync(int id)
        {
            var bloodBank =
                await _repository.GetByIdAsync(id);

            if (bloodBank == null)
                return null;

            return MapToDto(bloodBank);
        }

        // ==========================================
        // ADD BLOOD BANK
        // ==========================================

        public async Task<BloodBankSettingDto> AddAsync(
            UpdateBloodBankSettingDto dto)
        {
            var bloodBank = new BloodBankSetting
            {
                Name = dto.Name.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Address = dto.Address?.Trim() ?? string.Empty,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                UpdatedAt = DateTime.UtcNow
            };

            var created =
                await _repository.AddAsync(bloodBank);

            return MapToDto(created);
        }

        // ==========================================
        // UPDATE BLOOD BANK
        // ==========================================

        public async Task<BloodBankSettingDto?> UpdateAsync(
            int id,
            UpdateBloodBankSettingDto dto)
        {
            var bloodBank =
                await _repository.GetByIdAsync(id);

            if (bloodBank == null)
                return null;

            bloodBank.Name = dto.Name.Trim();
            bloodBank.PhoneNumber = dto.PhoneNumber.Trim();
            bloodBank.Address =
                dto.Address?.Trim() ?? string.Empty;
            bloodBank.Latitude = dto.Latitude;
            bloodBank.Longitude = dto.Longitude;
            bloodBank.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(bloodBank);

            return MapToDto(bloodBank);
        }

        // ==========================================
        // DELETE BLOOD BANK
        // ==========================================

        public async Task<bool> DeleteAsync(int id)
        {
            var bloodBank =
                await _repository.GetByIdAsync(id);

            if (bloodBank == null)
                return false;

            await _repository.DeleteAsync(bloodBank);

            return true;
        }

        // ==========================================
        // MAPPING
        // ==========================================

        private static BloodBankSettingDto MapToDto(
            BloodBankSetting bloodBank)
        {
            return new BloodBankSettingDto
            {
                Id = bloodBank.Id,
                Name = bloodBank.Name,
                PhoneNumber = bloodBank.PhoneNumber,
                Address = bloodBank.Address,
                Latitude = bloodBank.Latitude,
                Longitude = bloodBank.Longitude,
                UpdatedAt = bloodBank.UpdatedAt
            };
        }
    }
} 