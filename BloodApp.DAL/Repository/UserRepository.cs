using BloodDonationAPI.DAL.Data;
using BloodDonationAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.DAL.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Email.ToLower() == email.ToLower());
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<User>> GetDonorsAsync(
            string? bloodType = null,
            bool? availableToDonate = null)
        {
            var query = _context.Users
                .Where(x =>
                    x.IsActive &&
                    x.Role == "user");

            if (!string.IsNullOrWhiteSpace(bloodType))
            {
                query = query.Where(x =>
                    x.BloodType == bloodType);
            }

            if (availableToDonate.HasValue)
            {
                query = query.Where(x =>
                    x.AvailableToDonate ==
                    availableToDonate.Value);
            }

            return await query
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(x =>
                    x.Email.ToLower() ==
                    email.ToLower());
        }
    }
}