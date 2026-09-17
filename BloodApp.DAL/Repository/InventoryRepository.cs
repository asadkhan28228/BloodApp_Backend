using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class InventoryRepository
        : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(
            AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // INVENTORY
        // ==========================================

        public async Task<BloodInventory?>
            GetInventoryAsync(string bloodType)
        {
            return await _context.BloodInventory
                .FirstOrDefaultAsync(x =>
                    x.BloodType == bloodType);
        }

        public async Task<List<BloodInventory>>
            GetAllInventoryAsync()
        {
            return await _context.BloodInventory
                .OrderBy(x => x.BloodType)
                .ToListAsync();
        }

        public async Task<BloodInventory>
            AddInventoryAsync(
                BloodInventory inventory)
        {
            await _context.BloodInventory
                .AddAsync(inventory);

            await _context.SaveChangesAsync();

            return inventory;
        }

        public async Task UpdateInventoryAsync(
            BloodInventory inventory)
        {
            _context.BloodInventory.Update(inventory);

            await _context.SaveChangesAsync();
        }


        // ==========================================
        // BLOOD BATCHES
        // ==========================================

        public async Task<BloodBatch?>
            GetBatchAsync(Guid id)
        {
            return await _context.BloodBatches
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<BloodBatch>>
            GetAllBatchesAsync()
        {
            return await _context.BloodBatches
                .OrderBy(x => x.ExpiresAt)
                .ToListAsync();
        }

        public async Task<List<BloodBatch>>
            GetBatchesByBloodTypeAsync(
                string bloodType)
        {
            return await _context.BloodBatches
                .Where(x => x.BloodType == bloodType)
                .OrderBy(x => x.ExpiresAt)
                .ToListAsync();
        }

        public async Task<BloodBatch>
            AddBatchAsync(BloodBatch batch)
        {
            await _context.BloodBatches
                .AddAsync(batch);

            await _context.SaveChangesAsync();

            return batch;
        }

        public async Task UpdateBatchAsync(
            BloodBatch batch)
        {
            _context.BloodBatches.Update(batch);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteBatchAsync(
            BloodBatch batch)
        {
            _context.BloodBatches.Remove(batch);

            await _context.SaveChangesAsync();
        }
    }
}