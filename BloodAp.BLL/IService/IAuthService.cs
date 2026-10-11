
using BloodDonationAPI.BLL.DTOs.Auth;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IAuthService
    {
        // ==========================================
        // REGISTER
        // ==========================================

        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);

        // ==========================================
        // LOGIN
        // ==========================================

        Task<AuthResponseDto?> LoginAsync(LoginDto dto);

        // ==========================================
        // USER PROFILE
        // ==========================================

        Task<UserProfileDto?> GetProfileAsync(Guid userId);

        // ==========================================
        // FORGOT PASSWORD
        // ==========================================

        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);

        // ==========================================
        // RESET PASSWORD
        // ==========================================

        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);

        // ==========================================
        // REFRESH TOKEN
        // ==========================================

        Task<AuthResponseDto?> RefreshTokenAsync(string token);

        // ==========================================
        // LOGOUT
        // ==========================================

        Task<bool> LogoutAsync(string refreshToken);

        // ==========================================
        // EMAIL VERIFICATION - NEW
        // ==========================================

        Task<bool> VerifyEmailAsync(string token);

        // ==========================================
        // RESEND VERIFICATION EMAIL - NEW
        // ==========================================

        Task<bool> ResendVerificationEmailAsync(string email);
    }
}
