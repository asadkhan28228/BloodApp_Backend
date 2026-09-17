using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class CommunityNotification
    {
        [Key]
        public Guid Id { get; set; }

        // ==============================
        // TYPE
        // ==============================

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "community";

        // ==============================
        // NOTIFICATION CONTENT
        // ==============================

        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1500)]
        public string Message { get; set; } = string.Empty;

        public bool Unread { get; set; } = true;

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; }

        // ==============================
        // OPTIONAL REPORTER
        // ==============================

        public Guid? ReporterUid { get; set; }

        [MaxLength(150)]
        public string? ReporterName { get; set; }

        // ==============================
        // RELATED RECORD
        // ==============================

        public Guid? BloodRequestId { get; set; }

        public Guid? EmergencyRequestId { get; set; }

        // ==============================
        // EMERGENCY FIELDS
        // ==============================

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(30)]
        public string? Priority { get; set; }

        [MaxLength(10)]
        public string? BloodType { get; set; }

        public int? UnitsNeeded { get; set; }

        [MaxLength(50)]
        public string? BloodSource { get; set; }

        public int? ReservedUnits { get; set; }

        [MaxLength(300)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(2000)]
        public string? Details { get; set; }

        // ==============================
        // EXPIRATION
        // ==============================

        public DateTime? ExpireAt { get; set; }

        // ==============================
        // DATE
        // ==============================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}