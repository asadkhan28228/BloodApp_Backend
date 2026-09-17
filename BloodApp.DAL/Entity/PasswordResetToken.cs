using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class PasswordResetToken
    {
        [Key]
        public Guid Id { get; set; }

        // ==============================
        // USER
        // ==============================
        public Guid UserId { get; set; }

        // ==============================
        // TOKEN
        // ==============================
        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        // ==============================
        // EXPIRATION
        // ==============================
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        // ==============================
        // DATE
        // ==============================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ==============================
        // NAVIGATION
        // ==============================

        public User? User { get; set; }
    }
}