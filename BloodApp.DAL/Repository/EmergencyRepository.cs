using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class EmergencyRepository : IEmergencyRepository
    {
        private readonly AppDbContext _context;

        public EmergencyRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmergencyRequest?> GetByIdAsync(
            Guid id)
        {
            return await _context.EmergencyRequests
                .Include(x => x.Reporter)
                .Include(x => x.DonorResponses)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<EmergencyRequest>>
            GetAllAsync()
        {
            return await _context.EmergencyRequests
                .Include(x => x.Reporter)
                .Include(x => x.DonorResponses)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<EmergencyRequest>>
            GetActiveAsync()
        {
            return await _context.EmergencyRequests
                .Include(x => x.Reporter)
                .Include(x => x.DonorResponses)
                .Where(x => x.Status == "Active")
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<EmergencyRequest>>
            GetByReporterAsync(Guid reporterUid)
        {
            return await _context.EmergencyRequests
                .Include(x => x.DonorResponses)
                .Where(x => x.ReporterUid == reporterUid)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<EmergencyRequest> AddAsync(
            EmergencyRequest emergency)
        {
            await _context.EmergencyRequests
                .AddAsync(emergency);

            await _context.SaveChangesAsync();

            return emergency;
        }

        public async Task UpdateAsync(
            EmergencyRequest emergency)
        {
            _context.EmergencyRequests.Update(emergency);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            EmergencyRequest emergency)
        {
            _context.EmergencyRequests.Remove(emergency);

            await _context.SaveChangesAsync();
        }

        public async Task<EmergencyResponse?>
            GetResponseAsync(
                Guid emergencyId,
                Guid donorId)
        {
            return await _context.EmergencyResponses
                .FirstOrDefaultAsync(x =>
                    x.EmergencyRequestId == emergencyId &&
                    x.DonorId == donorId);
        }

        public async Task<List<EmergencyResponse>>
            GetResponsesAsync(Guid emergencyId)
        {
            return await _context.EmergencyResponses
                .Include(x => x.Donor)
                .Where(x =>
                    x.EmergencyRequestId == emergencyId)
                .OrderByDescending(x => x.RespondedAt)
                .ToListAsync();
        }

        public async Task<EmergencyResponse>
            AddResponseAsync(
                EmergencyResponse response)
        {
            await _context.EmergencyResponses
                .AddAsync(response);

            await _context.SaveChangesAsync();

            return response;
        }

        public async Task UpdateResponseAsync(
            EmergencyResponse response)
        {
            _context.EmergencyResponses.Update(response);

            await _context.SaveChangesAsync();
        }
    }
}