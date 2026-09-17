using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class BloodBatchRepository : IBloodBatchRepository
    {
        private readonly AppDbContext _context;

        public BloodBatchRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============================================
        // GET BY ID
        // ============================================

        public async Task<BloodBatch?> GetByIdAsync(Guid id)
        {
            return await _context.BloodBatches
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // ============================================
        // GET ALL
        // ============================================

        public async Task<List<BloodBatch>> GetAllAsync()
        {
            return await _context.BloodBatches
                .AsNoTracking()
                .OrderByDescending(x => x.AddedAt)
                .ToListAsync();
        }

        // ============================================
        // GET BY BLOOD TYPE
        // ============================================

        public async Task<List<BloodBatch>> GetByBloodTypeAsync(
            string bloodType)
        {
            return await _context.BloodBatches
                .AsNoTracking()
                .Where(x =>
                    x.BloodType == bloodType)
                .OrderBy(x => x.ExpiresAt)
                .ToListAsync();
        }

        // ============================================
        // GET ACTIVE BATCHES
        // ============================================

        public async Task<List<BloodBatch>> GetActiveBatchesAsync(
            string bloodType)
        {
            var now = DateTime.UtcNow;

            return await _context.BloodBatches
                .Where(x =>
                    x.BloodType == bloodType &&
                    x.Units > 0 &&
                    !x.IsExpired &&
                    x.ExpiresAt > now)
                .OrderBy(x => x.ExpiresAt)
                .ToListAsync();
        }

        // ============================================
        // ADD
        // ============================================

        public async Task<BloodBatch> AddAsync(
            BloodBatch batch)
        {
            await _context.BloodBatches
                .AddAsync(batch);

            await _context.SaveChangesAsync();

            return batch;
        }

        // ============================================
        // UPDATE
        // ============================================

        public async Task UpdateAsync(
            BloodBatch batch)
        {
            _context.BloodBatches
                .Update(batch);

            await _context.SaveChangesAsync();
        }

        // ============================================
        // DELETE
        // ============================================

        public async Task DeleteAsync(
            BloodBatch batch)
        {
            _context.BloodBatches
                .Remove(batch);

            await _context.SaveChangesAsync();
        }
    }
}