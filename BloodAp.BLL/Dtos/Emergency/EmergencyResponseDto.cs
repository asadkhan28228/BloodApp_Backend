namespace BloodDonationAPI.BLL.DTOs.Emergency
{
    public class EmergencyResponseDto
    {
        public Guid Id { get; set; }

        public Guid EmergencyRequestId { get; set; }

        public Guid DonorId { get; set; }

        public string DonorName { get; set; }
            = string.Empty;

        public string? DonorPhone { get; set; }

        public string? BloodType { get; set; }

        public string Status { get; set; }
            = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime RespondedAt { get; set; }

        public DateTime? OnTheWayAt { get; set; }

        public DateTime? ArrivedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}