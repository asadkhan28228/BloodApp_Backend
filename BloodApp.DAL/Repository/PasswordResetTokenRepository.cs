using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // ADD RESET TOKEN
        // ==========================================
        public async Task<PasswordResetToken> AddAsync(
            PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);

            await _context.SaveChangesAsync();

            return token;
        }

        // ==========================================
        // GET TOKEN
        // ==========================================
        public async Task<PasswordResetToken?> GetByTokenAsync(
            string token)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        // ==========================================
        // INVALIDATE OLD TOKENS
        // ==========================================
        public async Task InvalidateUserTokensAsync(
            Guid userId)
        {
            var tokens = await _context.PasswordResetTokens
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }

            if (tokens.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        // ==========================================
        // UPDATE TOKEN
        // ==========================================
        public async Task UpdateAsync(
            PasswordResetToken token)
        {
            _context.PasswordResetTokens.Update(token);

            await _context.SaveChangesAsync();
        }
    }
}