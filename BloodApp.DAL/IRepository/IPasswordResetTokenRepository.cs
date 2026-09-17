using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken> AddAsync(
            PasswordResetToken token);

        Task<PasswordResetToken?> GetByTokenAsync(
            string token);

        Task InvalidateUserTokensAsync(
            Guid userId);

        Task UpdateAsync(
            PasswordResetToken token);
    }
}