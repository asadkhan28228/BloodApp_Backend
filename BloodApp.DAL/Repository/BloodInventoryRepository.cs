using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class BloodInventoryRepository : IBloodInventoryRepository
    {
        private readonly AppDbContext _context;

        public BloodInventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BloodInventory>> GetAllAsync()
        {
            return await _context.BloodInventory.AsNoTracking()
                .OrderBy(x => x.BloodType).ToListAsync();
        }

        public async Task<BloodInventory?> GetByBloodTypeAsync(string bloodType)
        {
            return await _context.BloodInventory
                .FirstOrDefaultAsync(x => x.BloodType == bloodType);
        }

        public async Task<BloodInventory> AddAsync(BloodInventory inventory)
        {
            await _context.BloodInventory.AddAsync(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<BloodInventory> GetOrCreateAsync(string bloodType)
        {
            var inventory = await GetByBloodTypeAsync(bloodType);
            if (inventory != null) return inventory;

            inventory = new BloodInventory
            {
                BloodType = bloodType,
                Units = 0,
                UpdatedAt = DateTime.UtcNow
            };
            await _context.BloodInventory.AddAsync(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task IncreaseUnitsAsync(string bloodType, int units)
        {
            if (units <= 0) throw new ArgumentException("Units must be greater than zero.");
            var inventory = await GetOrCreateAsync(bloodType);
            inventory.Units += units;
            inventory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DecreaseUnitsAsync(string bloodType, int units)
        {
            if (units <= 0) return false;
            var changed = await _context.BloodInventory
                .Where(x => x.BloodType == bloodType && x.Units >= units)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Units, x => x.Units - units)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
            return changed == 1;
        }

        // Original all-or-nothing method remains for other callers.
        public async Task<bool> ReserveUnitsAsync(string bloodType, int units)
        {
            return await DecreaseUnitsAsync(bloodType, units);
        }

        // Reserve as much as is currently available, never exceeding requested units.
        // Conditional database UPDATE prevents two requests from taking the same units.
        public async Task<int> ReserveAvailableUnitsAsync(string bloodType, int units)
        {
            if (string.IsNullOrWhiteSpace(bloodType) || units <= 0) return 0;
            bloodType = bloodType.Trim().ToUpperInvariant();

            for (var attempt = 0; attempt < 5; attempt++)
            {
                var available = await _context.BloodInventory.AsNoTracking()
                    .Where(x => x.BloodType == bloodType)
                    .Select(x => (int?)x.Units)
                    .FirstOrDefaultAsync() ?? 0;

                var take = Math.Min(units, Math.Max(0, available));
                if (take == 0) return 0;

                var changed = await _context.BloodInventory
                    .Where(x => x.BloodType == bloodType && x.Units >= take)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Units, x => x.Units - take)
                        .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

                if (changed == 1) return take;
            }

            throw new InvalidOperationException("Inventory changed concurrently. Please retry.");
        }

        public async Task ReleaseUnitsAsync(string bloodType, int units)
        {
            if (units <= 0) return;
            var inventory = await GetOrCreateAsync(bloodType);
            inventory.Units += units;
            inventory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BloodInventory inventory)
        {
            inventory.UpdatedAt = DateTime.UtcNow;
            _context.BloodInventory.Update(inventory);
            await _context.SaveChangesAsync();
        }
    }
}
