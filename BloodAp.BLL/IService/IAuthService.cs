using BloodDonationAPI.BLL.DTOs.Auth;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);

        Task<AuthResponseDto?> LoginAsync(LoginDto dto);

        Task<UserProfileDto?> GetProfileAsync(Guid userId);

        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);

        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);

        Task<AuthResponseDto?> RefreshTokenAsync(string token);

        Task<bool> LogoutAsync(string refreshToken);
    }
}