using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class BloodBatch
    {
        [Key]
        public Guid Id { get; set; }

        // ==============================
        // BLOOD INFORMATION
        // ==============================

        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        public int Units { get; set; }

        // ==============================
        // EXPIRY
        // ==============================

        public DateTime ExpiresAt { get; set; }

        // ==============================
        // CREATED
        // ==============================

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public bool IsExpired { get; set; } = false;
    }
}