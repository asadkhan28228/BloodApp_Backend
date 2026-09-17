using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool AvailableToDonate { get; set; }

        public string? AvatarUrl { get; set; }

        public string? PushToken { get; set; }
    }
}