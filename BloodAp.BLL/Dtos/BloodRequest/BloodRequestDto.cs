namespace BloodDonationAPI.BLL.DTOs.BloodRequest
{
    public class BloodRequestDto
    {
        public Guid Id { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string BloodType { get; set; } = string.Empty;

        public string Urgency { get; set; } = string.Empty;

        public int UnitsNeeded { get; set; }

        public string HospitalName { get; set; } = string.Empty;

        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? ContactPhone { get; set; }

        public string? AdditionalNotes { get; set; }

        public string Status { get; set; } = string.Empty;

        public string RequestType { get; set; } = string.Empty;

        public string? BloodSource { get; set; }

        public Guid ReporterUid { get; set; }

        public string? ReporterName { get; set; }

        public Guid? EmergencyId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ExpireAt { get; set; }
    }
}