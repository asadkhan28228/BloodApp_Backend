namespace BloodDonationAPI.BLL.DTOs.BloodBatch
{
    public class BloodBatchDto
    {
        public Guid Id { get; set; }

        public string BloodType { get; set; }
            = string.Empty;

        public int Units { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime AddedAt { get; set; }

        public bool IsExpired { get; set; }
    }
}