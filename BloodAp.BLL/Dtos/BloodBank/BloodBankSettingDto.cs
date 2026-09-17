using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.BloodBank
{
    public class BloodBankSettingDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}