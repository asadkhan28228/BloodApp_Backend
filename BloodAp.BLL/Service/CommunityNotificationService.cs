using BloodDonationAPI.BLL.DTOs.Notification;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class CommunityNotificationService
        : ICommunityNotificationService
    {
        private readonly ICommunityNotificationRepository _repository;

        public CommunityNotificationService(
            ICommunityNotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CommunityNotificationDto>> GetAllAsync()
        {
            var notifications = await _repository.GetAllAsync();

            return notifications
                .Select(MapToDto)
                .ToList();
        }

        public async Task<CommunityNotificationDto?> GetByIdAsync(
            Guid id)
        {
            var notification =
                await _repository.GetByIdAsync(id);

            if (notification == null)
                return null;

            return MapToDto(notification);
        }

        public async Task<List<CommunityNotificationDto>>
            GetByReporterAsync(Guid reporterUid)
        {
            var notifications =
                await _repository.GetByReporterAsync(reporterUid);

            return notifications
                .Select(MapToDto)
                .ToList();
        }

        public async Task<CommunityNotificationDto> CreateAsync(
            CommunityNotificationDto dto)
        {
            var notification = new CommunityNotification
            {
                Id = Guid.NewGuid(),

                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,

                Unread = true,

                Icon = dto.Icon,
                Color = dto.Color,

                ReporterUid = dto.ReporterUid,
                ReporterName = dto.ReporterName,

                BloodRequestId = dto.BloodRequestId,
                EmergencyRequestId = dto.EmergencyRequestId,

                Status = dto.Status,
                Category = dto.Category,
                Priority = dto.Priority,

                BloodType = dto.BloodType,
                UnitsNeeded = dto.UnitsNeeded,

                BloodSource = dto.BloodSource,
                ReservedUnits = dto.ReservedUnits,

                Location = dto.Location,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,

                PhoneNumber = dto.PhoneNumber,
                Details = dto.Details,

                ExpireAt = dto.ExpireAt,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var created =
                await _repository.AddAsync(notification);

            return MapToDto(created);
        }

        public async Task<bool> MarkAsReadAsync(Guid id)
        {
            var notification =
                await _repository.GetByIdAsync(id);

            if (notification == null)
                return false;

            notification.Unread = false;
            notification.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(notification);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var notification =
                await _repository.GetByIdAsync(id);

            if (notification == null)
                return false;

            await _repository.DeleteAsync(notification);

            return true;
        }

        private static CommunityNotificationDto MapToDto(
            CommunityNotification notification)
        {
            return new CommunityNotificationDto
            {
                Id = notification.Id,

                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,

                Unread = notification.Unread,

                Icon = notification.Icon,
                Color = notification.Color,

                ReporterUid = notification.ReporterUid,
                ReporterName = notification.ReporterName,

                BloodRequestId = notification.BloodRequestId,
                EmergencyRequestId =
                    notification.EmergencyRequestId,

                Status = notification.Status,
                Category = notification.Category,
                Priority = notification.Priority,

                BloodType = notification.BloodType,
                UnitsNeeded = notification.UnitsNeeded,

                BloodSource = notification.BloodSource,
                ReservedUnits = notification.ReservedUnits,

                Location = notification.Location,
                Latitude = notification.Latitude,
                Longitude = notification.Longitude,

                PhoneNumber = notification.PhoneNumber,
                Details = notification.Details,

                ExpireAt = notification.ExpireAt,

                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt
            };
        }
    }
}