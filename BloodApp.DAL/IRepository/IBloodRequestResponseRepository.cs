using System;
using System.Collections.Generic;
using System.Text;

using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IBloodRequestResponseRepository
    {
        Task<BloodRequestResponse?> GetByIdAsync(Guid id);

        Task<BloodRequestResponse?> GetByRequestAndDonorAsync(
            Guid bloodRequestId,
            Guid donorId);

        Task<List<BloodRequestResponse>> GetByRequestAsync(
            Guid bloodRequestId);

        Task<BloodRequestResponse> AddAsync(
            BloodRequestResponse response);

        Task UpdateAsync(
            BloodRequestResponse response);
    }
}