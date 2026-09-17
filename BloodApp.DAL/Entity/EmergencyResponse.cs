using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class EmergencyResponse
    {
        [Key]
        public Guid Id { get; set; }

        public Guid EmergencyRequestId { get; set; }

        public Guid DonorId { get; set; }

        [MaxLength(150)]
        public string DonorName { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? DonorPhone { get; set; }

        [MaxLength(10)]
        public string? BloodType { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Accepted";

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;

        public DateTime? OnTheWayAt { get; set; }

        public DateTime? ArrivedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public EmergencyRequest? EmergencyRequest { get; set; }

        public User? Donor { get; set; }
    }
}