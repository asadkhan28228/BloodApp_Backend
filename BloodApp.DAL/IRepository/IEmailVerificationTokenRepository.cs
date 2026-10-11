
using BloodApp.DAL.Entity.BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IEmailVerificationTokenRepository
    {
        Task AddAsync(EmailVerificationToken token);

        Task<EmailVerificationToken?> GetByTokenHashAsync(
            string tokenHash);

        Task InvalidateUserTokensAsync(Guid userId);

        Task UpdateAsync(EmailVerificationToken token);
    }
}
