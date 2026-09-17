using System;
using System.Collections.Generic;
using System.Text;

namespace BloodDonationAPI.BLL.Dtos.Auth
{
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}