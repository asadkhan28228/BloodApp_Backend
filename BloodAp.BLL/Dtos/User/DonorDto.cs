namespace BloodDonationAPI.BLL.DTOs.User
{
    public class DonorDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string BloodType { get; set; } = string.Empty;

        public string? Gender { get; set; }

        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool AvailableToDonate { get; set; }

        public DateTime? LastDonationDate { get; set; }

        public int DonationsCount { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsActive { get; set; }

        public double? DistanceKm { get; set; }
    }
}