namespace BloodDonationAPI.BLL.DTOs.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Icon { get; set; }

        public string? Color { get; set; }

        public bool Unread { get; set; }

        public string? TargetScreen { get; set; }

        public string? ReferenceId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}