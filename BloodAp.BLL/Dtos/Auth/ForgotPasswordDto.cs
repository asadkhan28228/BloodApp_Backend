using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
    }
}