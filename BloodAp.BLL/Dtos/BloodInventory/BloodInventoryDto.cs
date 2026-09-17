using System;
using System.Collections.Generic;
using System.Text;

namespace BloodDonationAPI.BLL.DTOs.BloodInventory
{
    public class BloodInventoryDto
    {
        public string BloodType { get; set; } = string.Empty;

        public int Units { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}