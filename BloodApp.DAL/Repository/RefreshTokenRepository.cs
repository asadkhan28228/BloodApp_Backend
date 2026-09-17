using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ADD REFRESH TOKEN
        // =========================================================

        public async Task<RefreshToken> AddAsync(
            RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);

            await _context.SaveChangesAsync();

            return refreshToken;
        }

        // =========================================================
        // GET REFRESH TOKEN
        // =========================================================

        public async Task<RefreshToken?> GetByTokenAsync(
            string token)
        {
            return await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        // =========================================================
        // REVOKE ONE TOKEN
        // =========================================================

        public async Task RevokeAsync(
            RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;

            _context.RefreshTokens.Update(refreshToken);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // REVOKE ALL USER TOKENS
        // =========================================================

        public async Task RevokeAllByUserIdAsync(
            Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRevoked)
                .ToListAsync();

            if (tokens.Count == 0)
            {
                return;
            }

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}