using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        // ==============================
        // RECEIVER
        // ==============================

        public Guid UserId { get; set; }

        // ==============================
        // NOTIFICATION
        // ==============================

        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = "general";

        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; }

        public bool Unread { get; set; } = true;

        // Optional navigation target
        [MaxLength(300)]
        public string? TargetScreen { get; set; }

        [MaxLength(100)]
        public string? ReferenceId { get; set; }

        // ==============================
        // DATE
        // ==============================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // ==============================
        // NAVIGATION
        // ==============================

        public User? User { get; set; }
    }
}