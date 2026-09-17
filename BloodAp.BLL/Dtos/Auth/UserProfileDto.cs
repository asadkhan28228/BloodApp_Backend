namespace BloodDonationAPI.BLL.DTOs.Auth
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string BloodType { get; set; } = string.Empty;

        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool AvailableToDonate { get; set; }

        public DateTime? LastDonationDate { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Gender { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Role { get; set; } = "user";

        public bool IsActive { get; set; }

        public int DonationsCount { get; set; }

        public string? PushToken { get; set; }
    }
}