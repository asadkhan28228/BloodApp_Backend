
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IEmergencyResponseRepository
    {
        Task<EmergencyResponse?> GetByIdAsync(
            Guid id);

        Task<EmergencyResponse?> GetByEmergencyAndDonorAsync(
            Guid emergencyId,
            Guid donorId);

        Task<List<EmergencyResponse>> GetByEmergencyAsync(
            Guid emergencyId);

        Task<List<EmergencyResponse>> GetByDonorAsync(
            Guid donorId);

        Task<EmergencyResponse> AddAsync(
            EmergencyResponse response);

        Task UpdateAsync(
            EmergencyResponse response);

        Task DeleteAsync(
            EmergencyResponse response);

        // Only one donor can start the journey.
        // Returns false if another donor has
        // already started or arrived.
        Task<bool> TryStartJourneyAsync(
            Guid responseId,
            Guid donorId);
    }
}
