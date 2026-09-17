using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.Emergency
{
    public class CreateEmergencyRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Priority { get; set; } = "High";

        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [Range(1, 100)]
        public int UnitsNeeded { get; set; }

        [Required]
        [MaxLength(300)]
        public string Location { get; set; } = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [Required]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Details { get; set; }
    }
}