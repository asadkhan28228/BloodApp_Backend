
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

        // ============================================
        // FIRST DONOR STARTS JOURNEY
        // ============================================

        public async Task<bool> TryStartJourneyAsync(
            Guid responseId,
            Guid donorId)
        {
            var response = await _context.EmergencyResponses
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == responseId &&
                    x.DonorId == donorId);

            if (response == null)
                return false;

            var emergencyId = response.EmergencyRequestId;

            // SQL Server transaction protects
            // against two donors starting together.

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            // Lock emergency row before checking
            // and assigning a donor.
            //
            // Assumes SQL table name EmergencyRequests.

            var emergency = await _context.EmergencyRequests
                .FromSqlInterpolated(
                    $"SELECT * FROM [EmergencyRequests] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {emergencyId}")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (emergency == null ||
                !string.Equals(
                    emergency.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase) ||
                (emergency.ExpireAt.HasValue &&
                 emergency.ExpireAt.Value <= DateTime.UtcNow))
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Only an accepted response can
            // start a new journey.

            var currentResponse =
                await _context.EmergencyResponses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == responseId &&
                        x.DonorId == donorId &&
                        x.EmergencyRequestId == emergencyId);

            if (currentResponse == null ||
                !string.Equals(
                    currentResponse.Status,
                    "Accepted",
                    StringComparison.OrdinalIgnoreCase))
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Check if another donor already
            // started, arrived or completed.

            var alreadyAssigned =
                await _context.EmergencyResponses
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.EmergencyRequestId == emergencyId &&
                        x.Id != responseId &&
                        (x.Status == "On The Way" ||
                         x.Status == "Arrived" ||
                         x.Status == "Completed"));

            if (alreadyAssigned)
            {
                await transaction.RollbackAsync();
                return false;
            }

            var now = DateTime.UtcNow;

            // Conditional update:
            // only Accepted can become On The Way.

            var updated =
                await _context.EmergencyResponses
                    .Where(x =>
                        x.Id == responseId &&
                        x.DonorId == donorId &&
                        x.EmergencyRequestId == emergencyId &&
                        x.Status == "Accepted")
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            x => x.Status,
                            "On The Way")
                        .SetProperty(
                            x => x.OnTheWayAt,
                            (DateTime?)now)
                        .SetProperty(
                            x => x.UpdatedAt,
                            now));

            if (updated != 1)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();

            return true;
        }
    }
}
