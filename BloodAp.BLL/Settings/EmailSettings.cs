using System;
using System.Collections.Generic;
using System.Text;

namespace BloodDonationAPI.BLL.Settings
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;

        public int SmtpPort { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public string SenderEmail { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FrontendResetUrl { get; set; } = string.Empty;
    }
}