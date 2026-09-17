using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.BloodBatch
{
    public class UpdateBloodBatchDto
    {
        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [Range(0, 100000)]
        public int Units { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsExpired { get; set; }
    }
}