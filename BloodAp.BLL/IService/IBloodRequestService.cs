using BloodDonationAPI.BLL.DTOs.BloodRequest;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IBloodRequestService
    {
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
    }
}