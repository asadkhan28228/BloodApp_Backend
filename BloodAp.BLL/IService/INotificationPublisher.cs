namespace BloodDonationAPI.BLL.Interfaces
{
    public interface INotificationPublisher
    {
        Task PublishCommunityNotificationAsync(object notification);
        Task PublishUserNotificationAsync(Guid userId, object notification);
    }
}
