using System;
using System.Collections.Generic;
using System.Text;

namespace BloodDonationAPI.BLL.DTOs.BloodRequest
{
    public class BloodRequestResponseDto
    {
        public Guid Id { get; set; }

        public Guid BloodRequestId { get; set; }

        public Guid DonorId { get; set; }

        public string DonorName { get; set; } = string.Empty;

        public string? DonorPhone { get; set; }

        public string? BloodType { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}