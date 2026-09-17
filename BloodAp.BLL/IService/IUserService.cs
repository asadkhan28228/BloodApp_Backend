using BloodDonationAPI.BLL.DTOs.Auth;
using BloodDonationAPI.BLL.DTOs.User;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetByIdAsync(Guid userId);

        Task<List<UserProfileDto>> GetAllAsync();

            Task<List<DonorDto>> GetDonorsAsync(
            string? bloodType = null,
            bool? availableToDonate = null,
            double? latitude = null,
            double? longitude = null);

        Task<UserProfileDto?> UpdateProfileAsync(
            Guid userId,
            UpdateProfileDto dto);

        Task<bool> UpdatePushTokenAsync(
            Guid userId,
            string? pushToken);

        Task<bool> UpdateAvailabilityAsync(
            Guid userId,
            bool availableToDonate);

        Task<bool> DeactivateUserAsync(Guid userId);

        Task<bool> ActivateUserAsync(Guid userId);

        Task<bool> UpdateRoleAsync(Guid userId, string role);
    }
}