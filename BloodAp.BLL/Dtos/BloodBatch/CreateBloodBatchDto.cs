using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.BloodBatch
{
    public class CreateBloodBatchDto
    {
        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [Range(1, 100000)]
        public int Units { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}