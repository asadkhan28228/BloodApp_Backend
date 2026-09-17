using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class BloodRequestRepository : IBloodRequestRepository
    {
        private readonly AppDbContext _context;

        public BloodRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        public async Task<BloodRequest?> GetByIdAsync(Guid id)
        {
            return await _context.BloodRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // ==========================================
        // GET ALL
        // ==========================================

        public async Task<List<BloodRequest>> GetAllAsync()
        {
            return await _context.BloodRequests
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // ==========================================
        // GET BY REPORTER
        // ==========================================

        public async Task<List<BloodRequest>> GetByReporterAsync(
            Guid reporterUid)
        {
            return await _context.BloodRequests
                .AsNoTracking()
                .Where(x => x.ReporterUid == reporterUid)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // ==========================================
        // GET PENDING
        // ==========================================

        public async Task<List<BloodRequest>> GetPendingAsync()
        {
            return await _context.BloodRequests
                .AsNoTracking()
                .Where(x => x.Status == "Pending")
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }


        // ==========================================
        // GET BY EMERGENCY ID
        // ==========================================

        public async Task<BloodRequest?> GetByEmergencyIdAsync(
            Guid emergencyId)
        {
            return await _context.BloodRequests
                .FirstOrDefaultAsync(x => x.EmergencyId == emergencyId);
        }

        // ==========================================
        // ADD
        // ==========================================

        public async Task<BloodRequest> AddAsync(
            BloodRequest request)
        {
            await _context.BloodRequests.AddAsync(request);

            await _context.SaveChangesAsync();

            return request;
        }

        // ==========================================
        // UPDATE
        // ==========================================

        public async Task UpdateAsync(
            BloodRequest request)
        {
            _context.BloodRequests.Update(request);

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // DELETE
        // ==========================================

        public async Task DeleteAsync(
            BloodRequest request)
        {
            _context.BloodRequests.Remove(request);

            await _context.SaveChangesAsync();
        }
    }
}