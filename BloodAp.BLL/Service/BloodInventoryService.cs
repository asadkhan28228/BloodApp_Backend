using BloodDonationAPI.BLL.DTOs.BloodInventory;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class BloodInventoryService
        : IBloodInventoryService
    {
        private readonly IBloodInventoryRepository
            _inventoryRepository;

        public BloodInventoryService(
            IBloodInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        // ============================================
        // GET ALL INVENTORY
        // ============================================

        public async Task<List<BloodInventoryDto>> GetAllAsync()
        {
            var inventory =
                await _inventoryRepository.GetAllAsync();

            return inventory
                .Select(MapToDto)
                .ToList();
        }

        // ============================================
        // GET BY BLOOD TYPE
        // ============================================

        public async Task<BloodInventoryDto?>
            GetByBloodTypeAsync(string bloodType)
        {
            if (string.IsNullOrWhiteSpace(bloodType))
                return null;

            var inventory =
                await _inventoryRepository
                    .GetByBloodTypeAsync(
                        bloodType.Trim());

            if (inventory == null)
                return null;

            return MapToDto(inventory);
        }

        // ============================================
        // INCREASE UNITS
        // ============================================

        public async Task<bool> IncreaseUnitsAsync(
            string bloodType,
            int units)
        {
            if (string.IsNullOrWhiteSpace(bloodType))
                return false;

            if (units <= 0)
                return false;

            await _inventoryRepository
                .IncreaseUnitsAsync(
                    bloodType.Trim(),
                    units);

            return true;
        }

        // ============================================
        // DECREASE UNITS
        // ============================================

        public async Task<bool> DecreaseUnitsAsync(
            string bloodType,
            int units)
        {
            if (string.IsNullOrWhiteSpace(bloodType))
                return false;

            if (units <= 0)
                return false;

            return await _inventoryRepository
                .DecreaseUnitsAsync(
                    bloodType.Trim(),
                    units);
        }

        // ============================================
        // UPDATE INVENTORY
        // ============================================

        public async Task<bool> UpdateAsync(
            UpdateBloodInventoryDto dto)
        {
            if (dto == null)
                return false;

            if (string.IsNullOrWhiteSpace(dto.BloodType))
                return false;

            if (dto.Units < 0)
                return false;

            var bloodType =
                dto.BloodType.Trim();

            var inventory =
                await _inventoryRepository
                    .GetByBloodTypeAsync(
                        bloodType);

            // ========================================
            // CREATE NEW INVENTORY
            // ========================================

            if (inventory == null)
            {
                inventory = new BloodInventory
                {
                    BloodType = bloodType,
                    Units = dto.Units,
                    UpdatedAt = DateTime.UtcNow
                };

                await _inventoryRepository
                    .AddAsync(inventory);

                return true;
            }

            // ========================================
            // UPDATE EXISTING INVENTORY
            // ========================================

            inventory.Units = dto.Units;

            inventory.UpdatedAt =
                DateTime.UtcNow;

            await _inventoryRepository
                .UpdateAsync(inventory);

            return true;
        }

        // ============================================
        // MAP MODEL → DTO
        // ============================================

        private static BloodInventoryDto MapToDto(
            BloodInventory inventory)
        {
            return new BloodInventoryDto
            {
                BloodType = inventory.BloodType,
                Units = inventory.Units,
                UpdatedAt = inventory.UpdatedAt
            };
        }
    }
}