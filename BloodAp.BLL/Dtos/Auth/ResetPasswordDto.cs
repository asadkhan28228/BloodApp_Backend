using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}