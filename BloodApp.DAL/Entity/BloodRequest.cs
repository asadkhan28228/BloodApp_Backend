using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class BloodRequest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Urgency { get; set; } = "Medium";

        public int UnitsNeeded { get; set; }

        [Required]
        [MaxLength(200)]
        public string HospitalName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(30)]
        public string? ContactPhone { get; set; }

        [MaxLength(2000)]
        public string? AdditionalNotes { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        [Required]
        [MaxLength(30)]
        public string RequestType { get; set; } = "BloodRequest";

        // Blood Bank Reserve / Community Donors
        [MaxLength(50)]
        public string? BloodSource { get; set; }

        // ==============================
        // REPORTER
        // ==============================

        public Guid ReporterUid { get; set; }

        [MaxLength(150)]
        public string? ReporterName { get; set; }


        public Guid? EmergencyId { get; set; }

        // ==============================
        // DATES
        // ==============================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ExpireAt { get; set; }

        // ==============================
        // NAVIGATION
        // ==============================

        public User? Reporter { get; set; }

        public EmergencyRequest? Emergency { get; set; }
    }
}