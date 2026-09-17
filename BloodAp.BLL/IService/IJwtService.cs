
using BloodDonationAPI.DAL.Models;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);

        DateTime GetExpiration();
    }
}