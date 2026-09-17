using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class EmergencyRequestRepository
        : IEmergencyRequestRepository
    {
        private readonly AppDbContext _context;

        public EmergencyRequestRepository(
            AppDbContext context)
        {
            _context = context;
        }

        // ============================================
        // GET BY ID
        // ============================================

        public async Task<EmergencyRequest?> GetByIdAsync(
            Guid id)
        {
            return await _context.EmergencyRequests
                .Include(x => x.DonorResponses)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // ============================================
        // GET ALL
        // ============================================

        public async Task<List<EmergencyRequest>> GetAllAsync()
        {
            return await _context.EmergencyRequests
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // ============================================
        // GET ACTIVE
        // ============================================

        public async Task<List<EmergencyRequest>>
            GetActiveAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.EmergencyRequests
                .AsNoTracking()
                .Where(x =>
                    x.Status == "Active" &&
                    (x.ExpireAt == null ||
                     x.ExpireAt > now))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // ============================================
        // GET BY REPORTER
        // ============================================

        public async Task<List<EmergencyRequest>>
            GetByReporterAsync(Guid reporterUid)
        {
            return await _context.EmergencyRequests
                .AsNoTracking()
                .Where(x =>
                    x.ReporterUid == reporterUid)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // ============================================
        // ADD
        // ============================================

        public async Task<EmergencyRequest> AddAsync(
            EmergencyRequest emergency)
        {
            await _context.EmergencyRequests
                .AddAsync(emergency);

            await _context.SaveChangesAsync();

            return emergency;
        }

        // ============================================
        // UPDATE
        // ============================================

        public async Task UpdateAsync(
            EmergencyRequest emergency)
        {
            _context.EmergencyRequests
                .Update(emergency);

            await _context.SaveChangesAsync();
        }

        // ============================================
        // DELETE
        // ============================================

        public async Task DeleteAsync(
            EmergencyRequest emergency)
        {
            _context.EmergencyRequests
                .Remove(emergency);

            await _context.SaveChangesAsync();
        }
    }
}