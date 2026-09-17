using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class BloodInventory
    {
        [Key]
        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        public int Units { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}