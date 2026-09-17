using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.BloodRequest
{
    public class UpdateBloodRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Urgency { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string HospitalName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(30)]
        public string? ContactPhone { get; set; }

        [Range(1, 100)]
        public int UnitsNeeded { get; set; }

        [MaxLength(2000)]
        public string? AdditionalNotes { get; set; }
    }
}