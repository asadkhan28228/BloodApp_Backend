using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class BloodInventoryRepository
        : IBloodInventoryRepository
    {
        private readonly AppDbContext _context;

        public BloodInventoryRepository(
            AppDbContext context)
        {
            _context = context;
        }

        // ============================================
        // GET ALL INVENTORY
        // ============================================

        public async Task<List<BloodInventory>> GetAllAsync()
        {
            return await _context.BloodInventory
                .AsNoTracking()
                .OrderBy(x => x.BloodType)
                .ToListAsync();
        }

        // ============================================
        // GET BY BLOOD TYPE
        // ============================================

        public async Task<BloodInventory?> GetByBloodTypeAsync(
            string bloodType)
        {
            return await _context.BloodInventory
                .FirstOrDefaultAsync(
                    x => x.BloodType == bloodType);
        }
        // ============================================
        // ADD
        // ============================================

        public async Task<BloodInventory> AddAsync(
            BloodInventory inventory)
        {
            await _context.BloodInventory
                .AddAsync(inventory);

            await _context.SaveChangesAsync();

            return inventory;
        }
        // ============================================
        // GET OR CREATE
        // ============================================

        public async Task<BloodInventory> GetOrCreateAsync(
            string bloodType)
        {
            var inventory =
                await _context.BloodInventory
                    .FirstOrDefaultAsync(
                        x => x.BloodType == bloodType);

            if (inventory != null)
                return inventory;

            inventory = new BloodInventory
            {
                BloodType = bloodType,
                Units = 0,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.BloodInventory
                .AddAsync(inventory);

            await _context.SaveChangesAsync();

            return inventory;
        }

        // ============================================
        // INCREASE UNITS
        // ============================================

        public async Task IncreaseUnitsAsync(
            string bloodType,
            int units)
        {
            if (units <= 0)
                throw new ArgumentException(
                    "Units must be greater than zero.");

            var inventory =
                await GetOrCreateAsync(bloodType);

            inventory.Units += units;
            inventory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================
        // DECREASE UNITS
        // ============================================

        public async Task<bool> DecreaseUnitsAsync(
            string bloodType,
            int units)
        {
            if (units <= 0)
                return false;

            var inventory =
                await GetByBloodTypeAsync(bloodType);

            if (inventory == null)
                return false;

            if (inventory.Units < units)
                return false;

            inventory.Units -= units;
            inventory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================
        // RESERVE UNITS
        // ============================================

        public async Task<bool> ReserveUnitsAsync(
            string bloodType,
            int units)
        {
            if (units <= 0)
                return false;

            var inventory =
                await GetByBloodTypeAsync(bloodType);

            if (inventory == null)
                return false;

            if (inventory.Units < units)
                return false;

            inventory.Units -= units;
            inventory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================
        // RELEASE UNITS
        // ============================================

        public async Task ReleaseUnitsAsync(
            string bloodType,
            int units)
        {
            if (units <= 0)
                return;

            var inventory =
                await GetOrCreateAsync(bloodType);

            inventory.Units += units;
            inventory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================
        // UPDATE
        // ============================================

        public async Task UpdateAsync(
            BloodInventory inventory)
        {
            inventory.UpdatedAt = DateTime.UtcNow;

            _context.BloodInventory
                .Update(inventory);

            await _context.SaveChangesAsync();
        }
    }
}