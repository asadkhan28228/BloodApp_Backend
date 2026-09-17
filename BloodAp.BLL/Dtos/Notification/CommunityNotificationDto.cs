using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.Notification
{
    public class CommunityNotificationDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "community";

        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1500)]
        public string Message { get; set; } = string.Empty;

        public bool Unread { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; }

        public Guid? ReporterUid { get; set; }

        public string? ReporterName { get; set; }

        public Guid? BloodRequestId { get; set; }

        public Guid? EmergencyRequestId { get; set; }

        public string? Status { get; set; }

        public string? Category { get; set; }

        public string? Priority { get; set; }

        public string? BloodType { get; set; }

        public int? UnitsNeeded { get; set; }

        public string? BloodSource { get; set; }

        public int? ReservedUnits { get; set; }

        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Details { get; set; }

        public DateTime? ExpireAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}