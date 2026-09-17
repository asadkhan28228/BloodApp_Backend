using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class EmergencyResponseRepository
        : IEmergencyResponseRepository
    {
        private readonly AppDbContext _context;

        public EmergencyResponseRepository(
            AppDbContext context)
        {
            _context = context;
        }

        // ============================================
        // GET BY ID
        // ============================================

        public async Task<EmergencyResponse?> GetByIdAsync(
            Guid id)
        {
            return await _context.EmergencyResponses
                .Include(x => x.Donor)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // ============================================
        // GET BY EMERGENCY + DONOR
        // ============================================

        public async Task<EmergencyResponse?>
            GetByEmergencyAndDonorAsync(
                Guid emergencyId,
                Guid donorId)
        {
            return await _context.EmergencyResponses
                .FirstOrDefaultAsync(x =>
                    x.EmergencyRequestId == emergencyId &&
                    x.DonorId == donorId);
        }

        // ============================================
        // GET BY EMERGENCY
        // ============================================

        public async Task<List<EmergencyResponse>>
            GetByEmergencyAsync(
                Guid emergencyId)
        {
            return await _context.EmergencyResponses
                .AsNoTracking()
                .Where(x =>
                    x.EmergencyRequestId == emergencyId)
                .OrderByDescending(x => x.RespondedAt)
                .ToListAsync();
        }

        // ============================================
        // GET BY DONOR
        // ============================================

        public async Task<List<EmergencyResponse>>
            GetByDonorAsync(
                Guid donorId)
        {
            return await _context.EmergencyResponses
                .AsNoTracking()
                .Where(x =>
                    x.DonorId == donorId)
                .OrderByDescending(x => x.RespondedAt)
                .ToListAsync();
        }

        // ============================================
        // ADD
        // ============================================

        public async Task<EmergencyResponse> AddAsync(
            EmergencyResponse response)
        {
            await _context.EmergencyResponses
                .AddAsync(response);

            await _context.SaveChangesAsync();

            return response;
        }

        // ============================================
        // UPDATE
        // ============================================

        public async Task UpdateAsync(
            EmergencyResponse response)
        {
            _context.EmergencyResponses
                .Update(response);

            await _context.SaveChangesAsync();
        }

        // ============================================
        // DELETE
        // ============================================

        public async Task DeleteAsync(
            EmergencyResponse response)
        {
            _context.EmergencyResponses
                .Remove(response);

            await _context.SaveChangesAsync();
        }
    }
}