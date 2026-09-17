using BloodDonationAPI.BLL.DTOs.Emergency;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IEmergencyService
    {
        Task<EmergencyRequestDto?>
            GetByIdAsync(
                Guid id,
                Guid currentUserId,
                string currentUserRole);

        Task<List<EmergencyRequestDto>>
            GetAllAsync();

        Task<List<EmergencyRequestDto>>
            GetActiveAsync();

        Task<List<EmergencyRequestDto>>
            GetMyRequestsAsync(
                Guid currentUserId);

        Task<EmergencyRequestDto?>
            CreateAsync(
                CreateEmergencyRequestDto dto,
                Guid currentUserId);

        Task<bool>
            CancelAsync(
                Guid id,
                Guid currentUserId,
                string currentUserRole);

        Task<bool>
            ResolveAsync(
                Guid id,
                Guid currentUserId,
                string currentUserRole);

        Task<EmergencyResponseDto?>
            RespondAsync(
                Guid emergencyId,
                Guid donorId);

        Task<List<EmergencyResponseDto>>
            GetResponsesAsync(
                Guid emergencyId);

        Task<List<EmergencyResponseDto>>
            GetMyResponsesAsync(
                Guid donorId);

        Task<bool>
            UpdateResponseStatusAsync(
                Guid responseId,
                Guid donorId,
                string status);

        Task<bool>
            CompleteResponseAsync(
                Guid responseId,
                Guid donorId);
    }
}