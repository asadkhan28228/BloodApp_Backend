using System;
using System.Collections.Generic;
using System.Text;

using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IEmergencyResponseService
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

        Task<EmergencyResponse?> CreateAsync(
            EmergencyResponse response);

        Task<bool> UpdateAsync(
            EmergencyResponse response);

        Task<bool> DeleteAsync(
            Guid id);
    }
}