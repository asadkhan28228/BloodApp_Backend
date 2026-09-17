using BloodDonationAPI.BLL.DTOs.Auth;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IPasswordResetService
    {
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);

        Task<bool> VerifyOtpAsync(VerifyOtpDto dto);

        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}