
using System.Threading.Tasks;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IEmailService
    {
        // ==========================================
        // EMAIL VERIFICATION - NEW
        // ==========================================

        Task SendVerificationEmailAsync(
            string email,
            string fullName,
            string verificationLink);

        // ==========================================
        // PASSWORD RESET - EXISTING
        // ==========================================

        Task SendPasswordResetEmailAsync(
            string email,
            string resetLink);
    }
}
