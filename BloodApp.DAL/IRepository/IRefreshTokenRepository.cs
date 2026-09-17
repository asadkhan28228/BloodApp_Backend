using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> AddAsync(
            RefreshToken refreshToken);

        Task<RefreshToken?> GetByTokenAsync(
            string token);

        Task RevokeAsync(
            RefreshToken refreshToken);

        Task RevokeAllByUserIdAsync(
            Guid userId);
    }
}