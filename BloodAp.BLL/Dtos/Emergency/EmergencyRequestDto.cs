namespace BloodDonationAPI.BLL.DTOs.Emergency
{
    public class EmergencyRequestDto
    {
        public Guid Id { get; set; }

        public string Category { get; set; }
            = string.Empty;

        public string Priority { get; set; }
            = string.Empty;

        public string BloodType { get; set; }
            = string.Empty;

        public int UnitsNeeded { get; set; }

        public string Status { get; set; }
            = string.Empty;

        public string? BloodSource { get; set; }

        public int ReservedUnits { get; set; }

        public string Location { get; set; }
            = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string PhoneNumber { get; set; }
            = string.Empty;

        public string? Details { get; set; }

        public Guid ReporterUid { get; set; }

        public string ReporterName { get; set; }
            = string.Empty;

        public string Title { get; set; }
            = string.Empty;

        public string Message { get; set; }
            = string.Empty;

        public bool Unread { get; set; }

        public string? Icon { get; set; }

        public string? Color { get; set; }

        public DateTime? ExpireAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public int RespondingDonorsCount { get; set; }
    }
}