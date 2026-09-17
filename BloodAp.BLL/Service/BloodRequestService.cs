using BloodDonationAPI.BLL.DTOs.BloodRequest;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly IBloodRequestRepository _bloodRequestRepository;
        private readonly IUserRepository _userRepository;

        private readonly ICommunityNotificationRepository
            _communityNotificationRepository;

        private readonly INotificationPublisher
            _notificationPublisher;

        public BloodRequestService(
            IBloodRequestRepository bloodRequestRepository,
            IUserRepository userRepository,
            ICommunityNotificationRepository communityNotificationRepository,
            INotificationPublisher notificationPublisher)
        {
            _bloodRequestRepository =
                bloodRequestRepository;

            _userRepository =
                userRepository;

            _communityNotificationRepository =
                communityNotificationRepository;

            _notificationPublisher =
                notificationPublisher;
        }

        // ==========================================
        // GET REQUEST BY ID
        // ==========================================

        public async Task<BloodRequestDto?> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            string currentUserRole)
        {
            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(id);

            if (request == null)
                return null;

            // Normal user can only see his own request.
            // Admin and Blood Bank Admin can see all.
            if (!IsAdmin(currentUserRole) && request.ReporterUid != currentUserId)
            {
                return null;
            }

            return MapToDto(request);
        }

        // ==========================================
        // GET ALL
        // ==========================================

        public async Task<List<BloodRequestDto>> GetAllAsync()
        {
            var requests =await _bloodRequestRepository.GetAllAsync();

            return requests.Select(MapToDto).ToList();
        }

        // ==========================================
        // GET MY REQUESTS
        // ==========================================

        public async Task<List<BloodRequestDto>> GetMyRequestsAsync(Guid currentUserId)
        {
            var requests =await _bloodRequestRepository.GetByReporterAsync(currentUserId);

            return requests.Select(MapToDto).ToList();
        }

        // ==========================================
        // GET PENDING REQUESTS
        // ==========================================

        public async Task<List<BloodRequestDto>> GetPendingAsync()
        {
            var requests =await _bloodRequestRepository.GetPendingAsync();

            return requests.Select(MapToDto).ToList();
        }

        // ==========================================
        // CREATE BLOOD REQUEST
        // ==========================================

        public async Task<BloodRequestDto?> CreateAsync(CreateBloodRequestDto dto,Guid currentUserId)
        {
            // ------------------------------------------
            // Find logged-in user
            // ------------------------------------------

            var user =await _userRepository.GetByIdAsync(currentUserId);

            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            // ------------------------------------------
            // Create Blood Request
            // ------------------------------------------

            var request = new BloodRequest
            {
                Id = Guid.NewGuid(),

                PatientName =
                    dto.PatientName.Trim(),

                BloodType =
                    dto.BloodType.Trim(),

                Urgency =
                    dto.Urgency.Trim(),

                UnitsNeeded =
                    dto.UnitsNeeded,

                HospitalName =
                    dto.HospitalName.Trim(),

                Location =
                    dto.Location?.Trim(),

                Latitude =
                    dto.Latitude,

                Longitude =
                    dto.Longitude,

                ContactPhone =
                    dto.ContactPhone?.Trim(),

                AdditionalNotes =
                    dto.AdditionalNotes?.Trim(),

                Status = "Pending",

                RequestType = "BloodRequest",

                // Normal blood request
                // initially depends on community donors.
                BloodSource = "Community Donors",

                // Always use authenticated user.
                ReporterUid =
                    currentUserId,

                ReporterName =
                    user.FullName,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow,

                //ExpireAt = DateTime.UtcNow.AddDays(1)
                ExpireAt = DateTime.UtcNow.AddMinutes(3)
            };

            // ------------------------------------------
            // Save Blood Request
            // ------------------------------------------

            var createdRequest =
                await _bloodRequestRepository
                    .AddAsync(request);

            // ------------------------------------------
            // Create Community Notification
            // ------------------------------------------

            var unitText =
                createdRequest.UnitsNeeded == 1
                    ? "unit"
                    : "units";

            var notification =
                new CommunityNotification
                {
                    Id = Guid.NewGuid(),

                    Type = "request",

                    Title =
                        $"{createdRequest.BloodType} Blood Required 🩸",

                    Message =
                        $"{createdRequest.PatientName} needs " +
                        $"{createdRequest.UnitsNeeded} {unitText} " +
                        $"of {createdRequest.BloodType} at " +
                        $"{createdRequest.HospitalName}.",

                    Unread = true,

                    Icon = "water",

                    Color = "#DC2626",

                    ReporterUid =
                        createdRequest.ReporterUid,

                    ReporterName =
                        createdRequest.ReporterName,

                    // Link notification with request.
                    BloodRequestId =
                        createdRequest.Id,

                    EmergencyRequestId = null,

                    // Blood request information.
                    BloodType =
                        createdRequest.BloodType,

                    UnitsNeeded =
                        createdRequest.UnitsNeeded,

                    Priority =
                        createdRequest.Urgency,

                    // Location.
                    Location =
                        createdRequest.Location,

                    Latitude =
                        createdRequest.Latitude,

                    Longitude =
                        createdRequest.Longitude,

                    // Normal request does not reserve blood.
                    BloodSource =
                        createdRequest.BloodSource,

                    ReservedUnits = 0,

                    Status =
                        createdRequest.Status,

                    PhoneNumber =
                        createdRequest.ContactPhone,

                    Details =
                        createdRequest.AdditionalNotes,

                    ExpireAt = null,

                    CreatedAt =
                        DateTime.UtcNow,

                    UpdatedAt = createdRequest.ExpireAt,
                };

            // ------------------------------------------
            // Save Community Notification
            // ------------------------------------------

            await _communityNotificationRepository
                .AddAsync(notification);

            // ------------------------------------------
            // Send Real-Time SignalR Notification
            // ------------------------------------------

            await _notificationPublisher
                .PublishCommunityNotificationAsync(
                    notification);

            // ------------------------------------------
            // Return Created Request
            // ------------------------------------------

            return MapToDto(createdRequest);
        }

        // ==========================================
        // UPDATE REQUEST
        // ==========================================

        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateBloodRequestDto dto,
            Guid currentUserId,
            string currentUserRole)
        {
            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(id);

            if (request == null)
                return false;

            // Only owner or admin can update.
            if (!IsAdmin(currentUserRole) &&
                request.ReporterUid != currentUserId)
            {
                return false;
            }

            // Completed/cancelled cannot be edited.
            if (request.Status == "Completed" ||
                request.Status == "Cancelled")
            {
                return false;
            }

            request.PatientName =
                dto.PatientName.Trim();

            request.BloodType =
                dto.BloodType.Trim();

            request.Urgency =
                dto.Urgency.Trim();

            request.UnitsNeeded =
                dto.UnitsNeeded;

            request.HospitalName =
                dto.HospitalName.Trim();

            request.Location =
                dto.Location?.Trim();

            request.Latitude =
                dto.Latitude;

            request.Longitude =
                dto.Longitude;

            request.ContactPhone =
                dto.ContactPhone?.Trim();

            request.AdditionalNotes =
                dto.AdditionalNotes?.Trim();

            request.UpdatedAt =
                DateTime.UtcNow;

            await _bloodRequestRepository
                .UpdateAsync(request);

            return true;
        }

        // ==========================================
        // CANCEL REQUEST
        // ==========================================

        public async Task<bool> CancelAsync(
            Guid id,
            Guid currentUserId,
            string currentUserRole)
        {
            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(id);

            if (request == null)
                return false;

            // Only owner or admin.
            if (!IsAdmin(currentUserRole) &&
                request.ReporterUid != currentUserId)
            {
                return false;
            }

            if (request.Status == "Completed" ||
                request.Status == "Cancelled")
            {
                return false;
            }

            request.Status = "Cancelled";

            request.UpdatedAt =
                DateTime.UtcNow;

            await _bloodRequestRepository
                .UpdateAsync(request);

            return true;
        }

        // ==========================================
        // COMPLETE REQUEST
        // ==========================================

        public async Task<bool> CompleteAsync(
            Guid id,
            string currentUserRole)
        {
            // Only admin / blood bank admin.
            if (!IsAdmin(currentUserRole))
                return false;

            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(id);

            if (request == null)
                return false;

            if (request.Status == "Completed" ||
                request.Status == "Cancelled")
            {
                return false;
            }

            request.Status = "Completed";

            request.UpdatedAt =
                DateTime.UtcNow;

            await _bloodRequestRepository
                .UpdateAsync(request);

            return true;
        }

        // ==========================================
        // ADMIN ROLE CHECK
        // ==========================================

        private static bool IsAdmin(string role)
        {
            return role.Equals(
                       "admin",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   role.Equals(
                       "bloodBankAdmin",
                       StringComparison.OrdinalIgnoreCase);
        }

        // ==========================================
        // ENTITY -> DTO
        // ==========================================

        private static BloodRequestDto MapToDto(
            BloodRequest request)
        {
            return new BloodRequestDto
            {
                Id =
                    request.Id,

                PatientName =
                    request.PatientName,

                BloodType =
                    request.BloodType,

                Urgency =
                    request.Urgency,

                UnitsNeeded =
                    request.UnitsNeeded,

                HospitalName =
                    request.HospitalName,

                Location =
                    request.Location,

                Latitude =
                    request.Latitude,

                Longitude =
                    request.Longitude,

                ContactPhone =
                    request.ContactPhone,

                AdditionalNotes =
                    request.AdditionalNotes,

                Status =
                    request.Status,

                RequestType =
                    request.RequestType,

                BloodSource =
                    request.BloodSource,

                ReporterUid =
                    request.ReporterUid,

                ReporterName =
                    request.ReporterName,

                EmergencyId =
                    request.EmergencyId,

                CreatedAt =
                    request.CreatedAt,

                UpdatedAt =
                    request.UpdatedAt,

                ExpireAt =
                    request.ExpireAt
            };
        }
    }
}