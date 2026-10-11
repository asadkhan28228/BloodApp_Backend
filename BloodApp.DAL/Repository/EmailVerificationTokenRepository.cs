
using BloodApp.DAL.Entity.BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories
{
    public class EmailVerificationTokenRepository
        : IEmailVerificationTokenRepository
    {
        private readonly AppDbContext _context;

        public EmailVerificationTokenRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            EmailVerificationToken token)
        {
            await _context.EmailVerificationTokens
                .AddAsync(token);

            await _context.SaveChangesAsync();
        }

        public async Task<EmailVerificationToken?>
            GetByTokenHashAsync(string tokenHash)
        {
            return await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == tokenHash);
        }

        public async Task InvalidateUserTokensAsync(
            Guid userId)
        {
            var tokens = await _context.EmailVerificationTokens
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Update(token);

            await _context.SaveChangesAsync();
        }
    }
}
