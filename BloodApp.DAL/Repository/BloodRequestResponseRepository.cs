using System;
using System.Collections.Generic;
using System.Text;

using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class BloodRequestResponseRepository
        : IBloodRequestResponseRepository
    {
        private readonly AppDbContext _context;

        public BloodRequestResponseRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<BloodRequestResponse?> GetByIdAsync(Guid id)
        {
            return await _context.BloodRequestResponses
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BloodRequestResponse?>
            GetByRequestAndDonorAsync(
                Guid bloodRequestId,
                Guid donorId)
        {
            return await _context.BloodRequestResponses
                .FirstOrDefaultAsync(x =>
                    x.BloodRequestId == bloodRequestId &&
                    x.DonorId == donorId);
        }

        public async Task<List<BloodRequestResponse>>
            GetByRequestAsync(Guid bloodRequestId)
        {
            return await _context.BloodRequestResponses
                .AsNoTracking()
                .Where(x => x.BloodRequestId == bloodRequestId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<BloodRequestResponse> AddAsync(
            BloodRequestResponse response)
        {
            await _context.BloodRequestResponses.AddAsync(response);

            await _context.SaveChangesAsync();

            return response;
        }

        public async Task UpdateAsync(
            BloodRequestResponse response)
        {
            _context.BloodRequestResponses.Update(response);

            await _context.SaveChangesAsync();
        }
    }
}