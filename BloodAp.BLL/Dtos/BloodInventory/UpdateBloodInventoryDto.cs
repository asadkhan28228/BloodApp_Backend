using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.BLL.DTOs.BloodInventory
{
    public class UpdateBloodInventoryDto
    {
        [Required]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [Range(0, 100000)]
        public int Units { get; set; }
    }
}