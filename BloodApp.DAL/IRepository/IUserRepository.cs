using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.DAL.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByEmailAsync(string email);

        Task<List<User>> GetAllAsync();

        Task<List<User>> GetDonorsAsync(
            string? bloodType = null,
            bool? availableToDonate = null);

        Task<User> AddAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(User user);

        Task<bool> EmailExistsAsync(string email);
    }
}