using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class EmergencyRequest
    {
        [Key]
        public Guid Id { get; set; }

        // ==============================
        // EMERGENCY INFORMATION
        // ==============================

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Priority { get; set; } = "High";

        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        public int UnitsNeeded { get; set; }

        // ==============================
        // STATUS
        // ==============================

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        [MaxLength(50)]
        public string? BloodSource { get; set; }

        public int ReservedUnits { get; set; } = 0;

        // ==============================
        // LOCATION
        // ==============================

        [MaxLength(300)]
        public string Location { get; set; } = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Details { get; set; }

        // ==============================
        // REPORTER
        // ==============================

        public Guid ReporterUid { get; set; }

        [MaxLength(150)]
        public string ReporterName { get; set; } = string.Empty;

        // ==============================
        // NOTIFICATION DISPLAY
        // ==============================

        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public bool Unread { get; set; } = true;

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; }

        // ==============================
        // EXPIRATION
        // ==============================

        public DateTime? ExpireAt { get; set; }

        // ==============================
        // DATES
        // ==============================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        // ==============================
        // DONOR RESPONSE COUNT
        // ==============================

        public int RespondingDonorsCount { get; set; } = 0;

        // ==============================
        // NAVIGATION
        // ==============================

        public User? Reporter { get; set; }

        public ICollection<EmergencyResponse> DonorResponses { get; set; }
            = new List<EmergencyResponse>();

        public ICollection<BloodRequest> BloodRequests { get; set; }
            = new List<BloodRequest>();
    }
}