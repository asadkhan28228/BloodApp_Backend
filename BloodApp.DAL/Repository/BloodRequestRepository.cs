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

        public async Task<BloodRequest?> GetByIdAsync(Guid id)
        {
            return await _context.BloodRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<BloodRequest>> GetAllAsync()
        {
            return await _context.BloodRequests
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BloodRequest>> GetByReporterAsync(Guid reporterUid)
        {
            return await _context.BloodRequests
                .AsNoTracking()
                .Where(x => x.ReporterUid == reporterUid)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BloodRequest>> GetPendingAsync()
        {
            return await _context.BloodRequests
                .AsNoTracking()
                .Where(x => x.Status == "Pending")
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<BloodRequest?> GetByEmergencyIdAsync(Guid emergencyId)
        {
            return await _context.BloodRequests
                .FirstOrDefaultAsync(x => x.EmergencyId == emergencyId);
        }

        public async Task<BloodRequest> AddAsync(BloodRequest request)
        {
            await _context.BloodRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task UpdateAsync(BloodRequest request)
        {
            _context.BloodRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(BloodRequest request)
        {
            _context.BloodRequests.Remove(request);
            await _context.SaveChangesAsync();
        }

        // Delete only unanswered, ordinary blood requests after 24 hours.
        // A response of ANY status prevents automatic deletion.
        public async Task<int> DeleteExpiredAsync(DateTime utcNow)
        {
            //var cutoff = utcNow.AddHours(-24);

            var cutoff = utcNow.AddMinutes(-2);

            var expiredRequests = await _context.BloodRequests
                .Where(request =>
                    request.RequestType == "BloodRequest" &&
                    request.EmergencyId == null &&
                    request.Status == "Pending" &&
                    request.CreatedAt <= cutoff &&
                    !_context.BloodRequestResponses.Any(response =>
                        response.BloodRequestId == request.Id))
                .ToListAsync();

            if (expiredRequests.Count == 0)
                return 0;

            _context.BloodRequests.RemoveRange(expiredRequests);
            await _context.SaveChangesAsync();
            return expiredRequests.Count;
        }
    }
}
