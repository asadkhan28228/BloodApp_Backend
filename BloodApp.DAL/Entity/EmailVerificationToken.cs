using System;
using System.Collections.Generic;
using System.Text;

namespace BloodApp.DAL.Entity
{
    using global::BloodDonationAPI.DAL.Models;
    using System;
    using System.ComponentModel.DataAnnotations;

    namespace BloodDonationAPI.DAL.Models
    {
        public class EmailVerificationToken
        {
            [Key]
            public Guid Id { get; set; }

            [Required]
            public Guid UserId { get; set; }

            [Required]
            [MaxLength(64)]
            public string TokenHash { get; set; } = string.Empty;

            public DateTime ExpiresAt { get; set; }

            public bool IsUsed { get; set; } = false;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public User User { get; set; } = null!;
        }
    }

}
