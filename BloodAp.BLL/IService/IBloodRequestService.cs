using BloodDonationAPI.BLL.DTOs.BloodRequest;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IBloodRequestService
    {
        // ==========================================
        // BLOOD REQUESTS
        // ==========================================

        Task<BloodRequestDto?> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            string currentUserRole);

        Task<List<BloodRequestDto>> GetAllAsync();

        Task<List<BloodRequestDto>> GetMyRequestsAsync(
            Guid currentUserId);

        Task<List<BloodRequestDto>> GetPendingAsync();

        Task<BloodRequestDto?> CreateAsync(
            CreateBloodRequestDto dto,
            Guid currentUserId);

        Task<bool> UpdateAsync(
            Guid id,
            UpdateBloodRequestDto dto,
            Guid currentUserId,
            string currentUserRole);

        Task<bool> CancelAsync(
            Guid id,
            Guid currentUserId,
            string currentUserRole);

        Task<bool> CompleteAsync(
            Guid id,
            string currentUserRole);


        // ==========================================
        // DONOR RESPONSE
        // User B -> I Can Donate
        // ==========================================

        Task<BloodRequestResponseDto?> RespondAsync(
            Guid bloodRequestId,
            Guid donorId);


        // ==========================================
        // GET RESPONDERS
        // User A can see responder details
        // ==========================================

        Task<List<BloodRequestResponseDto>> GetResponsesAsync(
            Guid bloodRequestId,
            Guid currentUserId,
            string currentUserRole);


        // ==========================================
        // CONFIRM DONOR
        // User A confirms User B
        // ==========================================

        Task<BloodRequestResponseDto> ConfirmResponseAsync(
            Guid bloodRequestId,
            Guid responseId,
            Guid currentUserId,
            string currentUserRole);


        // ==========================================
        // REJECT DONOR
        // User A rejects donor response
        // ==========================================

        Task<BloodRequestResponseDto> RejectResponseAsync(
            Guid bloodRequestId,
            Guid responseId,
            Guid currentUserId,
            string currentUserRole);


        // ==========================================
        // COMPLETE DONATION
        // Updates request + response + donor history
        // ==========================================

        Task<BloodRequestResponseDto> CompleteDonationAsync(
            Guid bloodRequestId,
            Guid responseId,
            Guid currentUserId,
            string currentUserRole);
    }
}