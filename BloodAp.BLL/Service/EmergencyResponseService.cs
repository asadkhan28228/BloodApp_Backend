using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class EmergencyResponseService
        : IEmergencyResponseService
    {
        private readonly IEmergencyResponseRepository _repository;
        private readonly IEmergencyRequestRepository _emergencyRepository;
        private readonly IUserRepository _userRepository;

        public EmergencyResponseService(
            IEmergencyResponseRepository repository,
            IEmergencyRequestRepository emergencyRepository,
            IUserRepository userRepository)
        {
            _repository = repository;
            _emergencyRepository = emergencyRepository;
            _userRepository = userRepository;
        }

        // ============================================
        // GET BY ID
        // ============================================

        public async Task<EmergencyResponse?> GetByIdAsync(
            Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        // ============================================
        // GET BY EMERGENCY + DONOR
        // ============================================

        public async Task<EmergencyResponse?>
            GetByEmergencyAndDonorAsync(
                Guid emergencyId,
                Guid donorId)
        {
            return await _repository
                .GetByEmergencyAndDonorAsync(
                    emergencyId,
                    donorId);
        }

        // ============================================
        // GET ALL RESPONSES OF AN EMERGENCY
        // ============================================

        public async Task<List<EmergencyResponse>>
            GetByEmergencyAsync(
                Guid emergencyId)
        {
            return await _repository
                .GetByEmergencyAsync(emergencyId);
        }

        // ============================================
        // GET ALL RESPONSES OF A DONOR
        // ============================================

        public async Task<List<EmergencyResponse>>
            GetByDonorAsync(
                Guid donorId)
        {
            return await _repository
                .GetByDonorAsync(donorId);
        }

        // ============================================
        // CREATE RESPONSE
        // ============================================

        public async Task<EmergencyResponse?>
            CreateAsync(
                EmergencyResponse response)
        {
            // Check emergency exists
            var emergency = await _emergencyRepository
                .GetByIdAsync(response.EmergencyRequestId);

            if (emergency == null)
                return null;

            // Emergency must be active
            if (!string.Equals(
                    emergency.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Check donor exists
            var donor = await _userRepository
                .GetByIdAsync(response.DonorId);

            if (donor == null)
                return null;

            // Donor must be active
            if (!donor.IsActive)
                return null;

            // Donor must be available
            if (!donor.AvailableToDonate)
                return null;

            // Prevent duplicate response
            var existingResponse =
                await _repository
                    .GetByEmergencyAndDonorAsync(
                        response.EmergencyRequestId,
                        response.DonorId);

            if (existingResponse != null)
                return existingResponse;

            // Make sure ID exists
            if (response.Id == Guid.Empty)
                response.Id = Guid.NewGuid();

            // Make sure response time exists
            if (response.RespondedAt == default)
                response.RespondedAt = DateTime.UtcNow;

            return await _repository.AddAsync(response);
        }

        // ============================================
        // UPDATE RESPONSE
        // ============================================

        public async Task<bool> UpdateAsync(
            EmergencyResponse response)
        {
            var existingResponse =
                await _repository
                    .GetByIdAsync(response.Id);

            if (existingResponse == null)
                return false;

            existingResponse.EmergencyRequestId =
                response.EmergencyRequestId;

            existingResponse.DonorId =
                response.DonorId;

            existingResponse.RespondedAt =
                response.RespondedAt;

            await _repository.UpdateAsync(
                existingResponse);

            return true;
        }

        // ============================================
        // DELETE RESPONSE
        // ============================================

        public async Task<bool> DeleteAsync(
            Guid id)
        {
            var response =
                await _repository.GetByIdAsync(id);

            if (response == null)
                return false;

            await _repository.DeleteAsync(response);

            return true;
        }
    }
}