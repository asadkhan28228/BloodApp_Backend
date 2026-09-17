using System;
using System.Collections.Generic;
using System.Text;

namespace BloodDonationAPI.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(
            string email,
            string resetLink);
    }
}