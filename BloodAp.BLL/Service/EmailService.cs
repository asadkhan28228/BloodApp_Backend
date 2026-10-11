
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BloodDonationAPI.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ==========================================
        // SEND VERIFICATION EMAIL
        // ==========================================

        public async Task SendVerificationEmailAsync(
            string email,
            string fullName,
            string verificationLink)
        {
            var subject = "Verify Your Blood Care Account";

            var safeName = WebUtility.HtmlEncode(fullName);
            var safeLink = WebUtility.HtmlEncode(verificationLink);

            var body = $@"
                <html>
                <body style='font-family:Arial,sans-serif;
                             background-color:#f8fafc;
                             padding:24px;'>

                    <div style='max-width:520px;
                                margin:auto;
                                background:white;
                                padding:30px;
                                border-radius:12px;'>

                        <h2 style='color:#dc2626;'>
                            Blood Care
                        </h2>

                        <h3>Email Verification</h3>

                        <p>Hello {safeName},</p>

                        <p>
                            Thank you for registering with Blood Care.
                            Please verify your email address to
                            activate your account.
                        </p>

                        <p style='margin:30px 0;'>
                            <a href='{safeLink}'
                               style='background:#dc2626;
                                      color:white;
                                      padding:14px 24px;
                                      border-radius:8px;
                                      text-decoration:none;'>
                                Verify Email
                            </a>
                        </p>

                        <p>
                            This verification link will expire
                            after the configured time.
                        </p>

                        <p>
                            If you did not create this account,
                            you can ignore this email.
                        </p>

                        <hr/>

                        <p style='color:#64748b;font-size:12px;'>
                            Blood Care Team
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        // ==========================================
        // SEND PASSWORD RESET EMAIL
        // ==========================================

        public async Task SendPasswordResetEmailAsync(
            string email,
            string resetLink)
        {
            var subject = "Reset Your Blood Care Password";

            var safeLink = WebUtility.HtmlEncode(resetLink);

            var body = $@"
                <html>
                <body style='font-family:Arial,sans-serif;
                             background-color:#f8fafc;
                             padding:24px;'>

                    <div style='max-width:520px;
                                margin:auto;
                                background:white;
                                padding:30px;
                                border-radius:12px;'>

                        <h2 style='color:#dc2626;'>
                            Blood Care
                        </h2>

                        <h3>Password Reset</h3>

                        <p>
                            We received a request to reset
                            your Blood Care password.
                        </p>

                        <p style='margin:30px 0;'>
                            <a href='{safeLink}'
                               style='background:#dc2626;
                                      color:white;
                                      padding:14px 24px;
                                      border-radius:8px;
                                      text-decoration:none;'>
                                Reset Password
                            </a>
                        </p>

                        <p>
                            This password reset link will
                            expire after 30 minutes.
                        </p>

                        <p>
                            If you did not request a password
                            reset, ignore this email.
                        </p>

                        <hr/>

                        <p style='color:#64748b;font-size:12px;'>
                            Blood Care Team
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        // ==========================================
        // COMMON SMTP EMAIL SENDER
        // ==========================================

        private async Task SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlBody)
        {
            var host = _configuration["EmailSettings:SmtpHost"];
            var senderEmail =
                _configuration["EmailSettings:SenderEmail"];
            var password =
                _configuration["EmailSettings:Password"];
            var senderName =
                _configuration["EmailSettings:SenderName"]
                ?? "Blood Care";


            var port = 587;

            if (int.TryParse(
                _configuration["EmailSettings:SmtpPort"],
                out var configuredPort))
            {
                port = configuredPort;
            }

            var enableSsl = true;

            if (bool.TryParse(
                _configuration["EmailSettings:EnableSsl"],
                out var configuredSsl))
            {
                enableSsl = configuredSsl;
            }


            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "SMTP email settings are missing. " +
                    "Configure EmailSettings before sending emails.");
            }

            using var message = new MailMessage
            {
                From = new MailAddress(
                    senderEmail,
                    senderName),

                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(recipientEmail);

            using var smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(
                    senderEmail,
                    password),

                EnableSsl = enableSsl,
                DeliveryMethod =
                    SmtpDeliveryMethod.Network,

                UseDefaultCredentials = false
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}
