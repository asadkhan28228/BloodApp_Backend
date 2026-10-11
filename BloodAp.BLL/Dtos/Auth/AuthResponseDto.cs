
using System;

namespace BloodDonationAPI.BLL.DTOs.Auth
{
    public class AuthResponseDto
    {
        // JWT Access Token
        public string Token { get; set; } = string.Empty;

        // Refresh Token
        public string RefreshToken { get; set; } = string.Empty;

        // Token Expiry
        public DateTime ExpiresAt { get; set; }

        // User Profile
        public UserProfileDto User { get; set; } = new();

        // Email Verification Required
        public bool RequiresEmailVerification { get; set; } = false;

        // Authentication Status Message
        public string Message { get; set; } = string.Empty;
    }
}
