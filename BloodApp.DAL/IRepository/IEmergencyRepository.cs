using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IEmergencyRepository
    {
        Task<EmergencyRequest?> GetByIdAsync(Guid id);

        Task<List<EmergencyRequest>> GetAllAsync();

        Task<List<EmergencyRequest>> GetActiveAsync();

        Task<List<EmergencyRequest>> GetByReporterAsync(Guid reporterUid);

        Task<EmergencyRequest> AddAsync(
            EmergencyRequest emergency);

        Task UpdateAsync(
            EmergencyRequest emergency);

        Task DeleteAsync(
            EmergencyRequest emergency);


        // ==========================================
        // EMERGENCY RESPONSES
        // ==========================================

        Task<EmergencyResponse?> GetResponseAsync(
            Guid emergencyId,
            Guid donorId);

        Task<List<EmergencyResponse>> GetResponsesAsync(
            Guid emergencyId);

        Task<EmergencyResponse> AddResponseAsync(
            EmergencyResponse response);

        Task UpdateResponseAsync(
            EmergencyResponse response);
    }
}