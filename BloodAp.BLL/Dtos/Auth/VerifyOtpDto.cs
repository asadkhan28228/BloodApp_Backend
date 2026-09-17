using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.Auth
{
    public class VerifyOtpDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage = "OTP must be exactly 6 digits.")]
        public string Otp { get; set; } = string.Empty;
    }
}