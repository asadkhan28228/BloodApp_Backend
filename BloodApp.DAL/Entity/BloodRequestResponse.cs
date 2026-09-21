using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class BloodRequestResponse
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BloodRequestId { get; set; }

        public Guid DonorId { get; set; }

        [Required]
        [MaxLength(150)]
        public string DonorName { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? DonorPhone { get; set; }

        [MaxLength(10)]
        public string? BloodType { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public BloodRequest? BloodRequest { get; set; }

        public User? Donor { get; set; }
    }
}