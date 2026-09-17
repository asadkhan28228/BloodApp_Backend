using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DAL.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        // ==============================
        // BASIC USER INFORMATION
        // ==============================

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        // ==============================
        // BLOOD INFORMATION
        // ==============================

        [Required]
        [MaxLength(5)]
        public string BloodType { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Gender { get; set; }

        // ==============================
        // LOCATION
        // ==============================

        [MaxLength(200)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // ==============================
        // DONOR INFORMATION
        // ==============================

        public bool AvailableToDonate { get; set; } = true;

        public DateTime? LastDonationDate { get; set; }

        public int DonationsCount { get; set; } = 0;

        // ==============================
        // PROFILE
        // ==============================

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        // Expo Push Notification Token
        [MaxLength(500)]
        public string? PushToken { get; set; }

        // ==============================
        // ACCOUNT / ADMIN
        // ==============================

        [Required]
        [MaxLength(30)]
        public string Role { get; set; } = "user";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // ==============================
        // NAVIGATION PROPERTIES
        // ==============================

        public ICollection<BloodRequest> BloodRequests { get; set; }
            = new List<BloodRequest>();

        public ICollection<EmergencyRequest> EmergencyRequests { get; set; }
            = new List<EmergencyRequest>();

        public ICollection<EmergencyResponse> EmergencyResponses { get; set; }
            = new List<EmergencyResponse>();

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();
    }
}