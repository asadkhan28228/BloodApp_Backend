using BloodDonationAPI.BLL.DTOs.BloodBatch;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class BloodBatchService
        : IBloodBatchService
    {
        private readonly IBloodBatchRepository
            _batchRepository;

        private readonly IBloodInventoryRepository
            _inventoryRepository;

        public BloodBatchService(
            IBloodBatchRepository batchRepository,
            IBloodInventoryRepository inventoryRepository)
        {
            _batchRepository = batchRepository;
            _inventoryRepository = inventoryRepository;
        }

        // ============================================
        // GET ALL
        // ============================================

        public async Task<List<BloodBatchDto>> GetAllAsync()
        {
            await MarkExpiredBatchesAsync();

            var batches =
                await _batchRepository.GetAllAsync();

            return batches
                .Select(MapToDto)
                .ToList();
        }

        // ============================================
        // GET BY ID
        // ============================================

        public async Task<BloodBatchDto?> GetByIdAsync(
            Guid id)
        {
            var batch =
                await _batchRepository.GetByIdAsync(id);

            if (batch == null)
                return null;

            if (!batch.IsExpired &&
                batch.ExpiresAt <= DateTime.UtcNow)
            {
                batch.IsExpired = true;

                await _batchRepository
                    .UpdateAsync(batch);
            }

            return MapToDto(batch);
        }

        // ============================================
        // GET BY BLOOD TYPE
        // ============================================

        public async Task<List<BloodBatchDto>>
            GetByBloodTypeAsync(string bloodType)
        {
            if (string.IsNullOrWhiteSpace(bloodType))
                return new List<BloodBatchDto>();

            await MarkExpiredBatchesAsync();

            var batches =
                await _batchRepository
                    .GetByBloodTypeAsync(
                        bloodType.Trim());

            return batches
                .Select(MapToDto)
                .ToList();
        }

        // ============================================
        // ADD BATCH
        // ============================================

        public async Task<BloodBatchDto> AddAsync(
            CreateBloodBatchDto dto)
        {
            var bloodType =
                dto.BloodType.Trim();

            if (dto.ExpiresAt <= DateTime.UtcNow)
            {
                throw new ArgumentException(
                    "Expiry date must be in the future.");
            }

            var batch = new BloodBatch
            {
                Id = Guid.NewGuid(),

                BloodType = bloodType,

                Units = dto.Units,

                ExpiresAt = dto.ExpiresAt,

                AddedAt = DateTime.UtcNow,

                IsExpired = false
            };

            var createdBatch =
                await _batchRepository
                    .AddAsync(batch);

            // Add batch units to inventory
            await _inventoryRepository
                .IncreaseUnitsAsync(
                    bloodType,
                    dto.Units);

            return MapToDto(createdBatch);
        }

        // ============================================
        // UPDATE BATCH
        // ============================================

        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateBloodBatchDto dto)
        {
            var batch =
                await _batchRepository
                    .GetByIdAsync(id);

            if (batch == null)
                return false;

            var oldBloodType =
                batch.BloodType;

            var oldUnits =
                batch.Units;

            var newBloodType =
                dto.BloodType.Trim();

            if (dto.ExpiresAt <= DateTime.UtcNow &&
                !dto.IsExpired)
            {
                return false;
            }

            // ========================================
            // SAME BLOOD TYPE
            // ========================================

            if (oldBloodType == newBloodType)
            {
                var difference =
                    dto.Units - oldUnits;

                if (difference > 0)
                {
                    await _inventoryRepository
                        .IncreaseUnitsAsync(
                            newBloodType,
                            difference);
                }
                else if (difference < 0)
                {
                    var success =
                        await _inventoryRepository
                            .DecreaseUnitsAsync(
                                newBloodType,
                                Math.Abs(difference));

                    if (!success)
                        return false;
                }
            }

            // ========================================
            // BLOOD TYPE CHANGED
            // ========================================

            else
            {
                if (oldUnits > 0)
                {
                    await _inventoryRepository
                        .DecreaseUnitsAsync(
                            oldBloodType,
                            oldUnits);
                }

                if (dto.Units > 0)
                {
                    await _inventoryRepository
                        .IncreaseUnitsAsync(
                            newBloodType,
                            dto.Units);
                }
            }

            batch.BloodType =
                newBloodType;

            batch.Units =
                dto.Units;

            batch.ExpiresAt =
                dto.ExpiresAt;

            batch.IsExpired =
                dto.IsExpired;

            await _batchRepository
                .UpdateAsync(batch);

            return true;
        }

        // ============================================
        // DELETE BATCH
        // ============================================

        public async Task<bool> DeleteAsync(
            Guid id)
        {
            var batch =
                await _batchRepository
                    .GetByIdAsync(id);

            if (batch == null)
                return false;

            // Remove remaining units from inventory
            if (batch.Units > 0 &&
                !batch.IsExpired &&
                batch.ExpiresAt > DateTime.UtcNow)
            {
                var success =
                    await _inventoryRepository
                        .DecreaseUnitsAsync(
                            batch.BloodType,
                            batch.Units);

                if (!success)
                    return false;
            }

            await _batchRepository
                .DeleteAsync(batch);

            return true;
        }

        // ============================================
        // MARK EXPIRED BATCHES
        // ============================================

        public async Task<int> MarkExpiredBatchesAsync()
        {
            var batches =
                await _batchRepository
                    .GetAllAsync();

            var now =
                DateTime.UtcNow;

            int expiredCount = 0;

            foreach (var batch in batches)
            {
                if (batch.IsExpired)
                    continue;

                if (batch.ExpiresAt > now)
                    continue;

                batch.IsExpired = true;

                // Remove expired units from inventory
                if (batch.Units > 0)
                {
                    await _inventoryRepository
                        .DecreaseUnitsAsync(
                            batch.BloodType,
                            batch.Units);
                }

                await _batchRepository
                    .UpdateAsync(batch);

                expiredCount++;
            }

            return expiredCount;
        }

        // ============================================
        // MAP MODEL → DTO
        // ============================================

        private static BloodBatchDto MapToDto(
            BloodBatch batch)
        {
            return new BloodBatchDto
            {
                Id = batch.Id,

                BloodType =
                    batch.BloodType,

                Units =
                    batch.Units,

                ExpiresAt =
                    batch.ExpiresAt,

                AddedAt =
                    batch.AddedAt,

                IsExpired =
                    batch.IsExpired
            };
        }
    }
}