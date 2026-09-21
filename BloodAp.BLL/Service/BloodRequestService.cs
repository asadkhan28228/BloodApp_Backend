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
        private readonly IBloodRequestResponseRepository _bloodRequestResponseRepository;

        private readonly INotificationRepository _notificationRepository;

        private readonly ICommunityNotificationRepository _communityNotificationRepository;

        private readonly INotificationPublisher _notificationPublisher;

        public BloodRequestService(
            IBloodRequestRepository bloodRequestRepository,
            IUserRepository userRepository,
            ICommunityNotificationRepository communityNotificationRepository,
            INotificationPublisher notificationPublisher,
            IBloodRequestResponseRepository bloodRequestResponseRepository,
            INotificationRepository notificationRepository)
        {
            _bloodRequestRepository = bloodRequestRepository;

            _userRepository = userRepository;

            _communityNotificationRepository = communityNotificationRepository;

            _notificationPublisher = notificationPublisher;

            _bloodRequestResponseRepository = bloodRequestResponseRepository;

            _notificationRepository = notificationRepository;
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
            var requests = await _bloodRequestRepository.GetAllAsync();

            return requests.Select(MapToDto).ToList();
        }

        // ==========================================
        // GET MY REQUESTS
        // ==========================================

        public async Task<List<BloodRequestDto>> GetMyRequestsAsync(Guid currentUserId)
        {
            var requests = await _bloodRequestRepository.GetByReporterAsync(currentUserId);

            return requests.Select(MapToDto).ToList();
        }

        // ==========================================
        // GET PENDING REQUESTS
        // ==========================================

        public async Task<List<BloodRequestDto>> GetPendingAsync()
        {
            var requests = await _bloodRequestRepository.GetPendingAsync();

            return requests.Select(MapToDto).ToList();
        }

        // ==========================================
        // CREATE BLOOD REQUEST
        // ==========================================

        public async Task<BloodRequestDto?> CreateAsync(CreateBloodRequestDto dto, Guid currentUserId)
        {
            // ------------------------------------------
            // Find logged-in user
            // ------------------------------------------

            var user = await _userRepository.GetByIdAsync(currentUserId);

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

                ExpireAt = DateTime.UtcNow.AddDays(1)
                // ExpireAt = DateTime.UtcNow.AddMinutes(3)
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
        // RESPOND TO BLOOD REQUEST
        // ==========================================

        public async Task<BloodRequestResponseDto?> RespondAsync(
            Guid bloodRequestId,
            Guid donorId)
        {
            // ------------------------------------------
            // Find blood request
            // ------------------------------------------

            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(bloodRequestId);

            if (request == null)
                return null;

            // Only pending requests accept donors.
            if (!request.Status.Equals(
                    "Pending",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This blood request is no longer accepting donors.");
            }

            // User cannot donate to own request.
            if (request.ReporterUid == donorId)
            {
                throw new InvalidOperationException(
                    "You cannot respond to your own blood request.");
            }

            // ------------------------------------------
            // Find donor
            // ------------------------------------------

            var donor =
                await _userRepository
                    .GetByIdAsync(donorId);

            if (donor == null)
            {
                throw new InvalidOperationException(
                    "Donor profile was not found.");
            }

            if (!donor.IsActive)
            {
                throw new InvalidOperationException(
                    "Your account is currently inactive.");
            }

            if (!donor.AvailableToDonate)
            {
                throw new InvalidOperationException(
                    "Your donor profile is currently marked unavailable.");
            }

            // ------------------------------------------
            // Blood type check
            // ------------------------------------------

            if (string.IsNullOrWhiteSpace(donor.BloodType))
            {
                throw new InvalidOperationException(
                    "Please add your blood type to your profile first.");
            }

            if (!string.Equals(
                    donor.BloodType.Trim(),
                    request.BloodType.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"This request requires {request.BloodType} blood. " +
                    $"Your profile blood type is {donor.BloodType}.");
            }

            // ------------------------------------------
            // Duplicate response check
            // ------------------------------------------

            var existingResponse =
                await _bloodRequestResponseRepository
                    .GetByRequestAndDonorAsync(
                        bloodRequestId,
                        donorId);

            if (existingResponse != null)
            {
                throw new InvalidOperationException(
                    "You have already responded to this blood request.");
            }

            // ------------------------------------------
            // Create donor response
            // ------------------------------------------

            var response = new BloodRequestResponse
            {
                Id = Guid.NewGuid(),

                BloodRequestId =
                    bloodRequestId,

                DonorId =
                    donorId,

                DonorName =
                    donor.FullName,

                DonorPhone =
                    donor.PhoneNumber,

                BloodType =
                    donor.BloodType,

                Status =
                    "Pending",

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

            await _bloodRequestResponseRepository
                .AddAsync(response);

            // ------------------------------------------
            // Private notification for requester
            // ------------------------------------------

            var notification = new Notification
            {
                Id = Guid.NewGuid(),

                UserId =
                    request.ReporterUid,

                Type =
                    "blood_request_response",

                Title =
                    "New Blood Donor ❤️",

                Message =
                    $"{donor.FullName} ({donor.BloodType}) " +
                    $"is willing to donate blood for {request.PatientName}.",

                Icon =
                    "heart",

                Color =
                    "#DC2626",

                Unread =
                    true,

                TargetScreen =
                    "notifications",

                // Request ID is kept here so frontend can
                // load donor responses for this request.
                ReferenceId =
                    request.Id.ToString(),

                CreatedAt =
                    DateTime.UtcNow
            };

            await _notificationRepository
                .AddAsync(notification);

            // ------------------------------------------
            // Real-time private SignalR notification
            // ------------------------------------------

            await _notificationPublisher
                .PublishUserNotificationAsync(
                    request.ReporterUid,
                    notification);

            return MapResponseToDto(response);
        }


        // ==========================================
        // GET RESPONSES FOR REQUEST
        // ==========================================

        public async Task<List<BloodRequestResponseDto>>
            GetResponsesAsync(
                Guid bloodRequestId,
                Guid currentUserId,
                string currentUserRole)
        {
            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(bloodRequestId);

            if (request == null)
            {
                throw new KeyNotFoundException(
                    "Blood request not found.");
            }

            // Only requester/admin can see donor personal details.
            if (!IsAdmin(currentUserRole) &&
                request.ReporterUid != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to view donor responses.");
            }

            var responses =
                await _bloodRequestResponseRepository
                    .GetByRequestAsync(bloodRequestId);

            return responses
                .Select(MapResponseToDto)
                .ToList();
        }
        // ==========================================
        // CONFIRM DONOR RESPONSE
        // ==========================================

        public async Task<BloodRequestResponseDto> ConfirmResponseAsync(
            Guid bloodRequestId,
            Guid responseId,
            Guid currentUserId,
            string currentUserRole)
        {
            // ------------------------------------------
            // Find blood request
            // ------------------------------------------

            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(bloodRequestId);

            if (request == null)
            {
                throw new KeyNotFoundException(
                    "Blood request not found.");
            }

            // Only requester or admin can confirm donor.
            if (!IsAdmin(currentUserRole) &&
                request.ReporterUid != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to confirm a donor for this request.");
            }

            if (request.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase) ||
                request.Status.Equals(
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This blood request is already closed.");
            }

            // ------------------------------------------
            // Find donor response
            // ------------------------------------------

            var response =
                await _bloodRequestResponseRepository
                    .GetByIdAsync(responseId);

            if (response == null ||
                response.BloodRequestId != bloodRequestId)
            {
                throw new KeyNotFoundException(
                    "Donor response not found.");
            }

            if (response.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This donation has already been completed.");
            }

            if (response.Status.Equals(
                    "Rejected",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "A rejected donor response cannot be confirmed.");
            }

            if (response.Status.Equals(
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This donor has already been confirmed.");
            }

            // ------------------------------------------
            // Check if another donor is already confirmed
            // ------------------------------------------

            var allResponses =
                await _bloodRequestResponseRepository
                    .GetByRequestAsync(bloodRequestId);

            var alreadyConfirmed =
                allResponses.Any(x =>
                    x.Id != response.Id &&
                    (
                        x.Status.Equals(
                            "Confirmed",
                            StringComparison.OrdinalIgnoreCase) ||
                        x.Status.Equals(
                            "Completed",
                            StringComparison.OrdinalIgnoreCase)
                    ));

            if (alreadyConfirmed)
            {
                throw new InvalidOperationException(
                    "Another donor has already been confirmed for this request.");
            }

            // ------------------------------------------
            // Confirm response
            // ------------------------------------------

            response.Status = "Confirmed";
            response.UpdatedAt = DateTime.UtcNow;

            await _bloodRequestResponseRepository
                .UpdateAsync(response);

            // Request is no longer open for new donors.
            request.Status = "Matched";
            request.UpdatedAt = DateTime.UtcNow;

            await _bloodRequestRepository
                .UpdateAsync(request);

            // ------------------------------------------
            // Update donor stats immediately on confirm
            // ------------------------------------------

            var donor =
                await _userRepository
                    .GetByIdAsync(response.DonorId);

            if (donor == null)
            {
                throw new KeyNotFoundException(
                    "Donor profile not found.");
            }

            donor.DonationsCount += 1;
            donor.LivesSaved += 1;
            donor.LastDonationDate = DateTime.UtcNow;
            donor.UpdatedAt = DateTime.UtcNow;

            await _userRepository
                .UpdateAsync(donor);

            // ------------------------------------------
            // Notify donor
            // ------------------------------------------

            var notification = new Notification
            {
                Id = Guid.NewGuid(),

                UserId = response.DonorId,

                Type = "blood_request_confirmed",

                Title = "Donation Confirmed ❤️",

                Message =
                    $"Your donation offer for {request.PatientName} " +
                    $"has been confirmed by the requester.",

                Icon = "checkmark-circle",

                Color = "#16A34A",

                Unread = true,

                TargetScreen = "notifications",

                ReferenceId = request.Id.ToString(),

                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository
                .AddAsync(notification);

            await _notificationPublisher
                .PublishUserNotificationAsync(
                    response.DonorId,
                    notification);

            return MapResponseToDto(response);
        }


        // ==========================================
        // REJECT DONOR RESPONSE
        // ==========================================

        public async Task<BloodRequestResponseDto> RejectResponseAsync(
            Guid bloodRequestId,
            Guid responseId,
            Guid currentUserId,
            string currentUserRole)
        {
            // ------------------------------------------
            // Find blood request
            // ------------------------------------------

            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(bloodRequestId);

            if (request == null)
            {
                throw new KeyNotFoundException(
                    "Blood request not found.");
            }

            // Only requester or admin can reject.
            if (!IsAdmin(currentUserRole) &&
                request.ReporterUid != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to reject donors for this request.");
            }

            if (request.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase) ||
                request.Status.Equals(
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This blood request is already closed.");
            }

            // ------------------------------------------
            // Find response
            // ------------------------------------------

            var response =
                await _bloodRequestResponseRepository
                    .GetByIdAsync(responseId);

            if (response == null ||
                response.BloodRequestId != bloodRequestId)
            {
                throw new KeyNotFoundException(
                    "Donor response not found.");
            }

            if (response.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "A completed donation cannot be rejected.");
            }

            if (response.Status.Equals(
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "A confirmed donor cannot be rejected from this action.");
            }

            if (response.Status.Equals(
                    "Rejected",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This donor response has already been rejected.");
            }

            // ------------------------------------------
            // Reject response
            // ------------------------------------------

            response.Status = "Rejected";
            response.UpdatedAt = DateTime.UtcNow;

            await _bloodRequestResponseRepository
                .UpdateAsync(response);

            // ------------------------------------------
            // Notify donor
            // ------------------------------------------

            var notification = new Notification
            {
                Id = Guid.NewGuid(),

                UserId = response.DonorId,

                Type = "blood_request_rejected",

                Title = "Donation Response Update",

                Message =
                    $"Your donation offer for {request.PatientName} " +
                    $"was not selected for this request.",

                Icon = "information-circle",

                Color = "#6B7280",

                Unread = true,

                TargetScreen = "notifications",

                ReferenceId = request.Id.ToString(),

                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository
                .AddAsync(notification);

            await _notificationPublisher
                .PublishUserNotificationAsync(
                    response.DonorId,
                    notification);

            return MapResponseToDto(response);
        }


        // ==========================================
        // COMPLETE DONATION
        // ==========================================

        public async Task<BloodRequestResponseDto> CompleteDonationAsync(
            Guid bloodRequestId,
            Guid responseId,
            Guid currentUserId,
            string currentUserRole)
        {
            // ------------------------------------------
            // Find request
            // ------------------------------------------

            var request =
                await _bloodRequestRepository
                    .GetByIdAsync(bloodRequestId);

            if (request == null)
            {
                throw new KeyNotFoundException(
                    "Blood request not found.");
            }

            // Requester or admin can complete donation.
            if (!IsAdmin(currentUserRole) &&
                request.ReporterUid != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to complete this donation.");
            }

            if (request.Status.Equals(
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "A cancelled blood request cannot be completed.");
            }

            // ------------------------------------------
            // Find response
            // ------------------------------------------

            var response =
                await _bloodRequestResponseRepository
                    .GetByIdAsync(responseId);

            if (response == null ||
                response.BloodRequestId != bloodRequestId)
            {
                throw new KeyNotFoundException(
                    "Donor response not found.");
            }

            // IMPORTANT:
            // prevents donation count from increasing twice.
            if (response.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This donation has already been completed.");
            }

            if (!response.Status.Equals(
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The donor must be confirmed before completing the donation.");
            }

            // ------------------------------------------
            // Find donor
            // ------------------------------------------

            var donor =
                await _userRepository
                    .GetByIdAsync(response.DonorId);

            if (donor == null)
            {
                throw new KeyNotFoundException(
                    "Donor profile not found.");
            }

            // ------------------------------------------
            // Complete response
            // ------------------------------------------

            response.Status = "Completed";
            response.UpdatedAt = DateTime.UtcNow;

            await _bloodRequestResponseRepository
                .UpdateAsync(response);

            // ------------------------------------------
            // Complete blood request
            // ------------------------------------------

            request.Status = "Completed";
            request.UpdatedAt = DateTime.UtcNow;

            await _bloodRequestRepository
                .UpdateAsync(request);

            // ------------------------------------------
            // Notify donor
            // ------------------------------------------

            var notification = new Notification
            {
                Id = Guid.NewGuid(),

                UserId = donor.Id,

                Type = "donation_completed",

                Title = "Donation Completed ❤️",

                Message =
                    $"Thank you {donor.FullName}! Your blood donation " +
                    $"for {request.PatientName} has been completed successfully.",

                Icon = "heart-circle",

                Color = "#DC2626",

                Unread = true,

                TargetScreen = "notifications",

                ReferenceId = request.Id.ToString(),

                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository
                .AddAsync(notification);

            await _notificationPublisher
                .PublishUserNotificationAsync(
                    donor.Id,
                    notification);

            return MapResponseToDto(response);
        }

        // ==========================================
        // RESPONSE -> DTO
        // ==========================================

        private static BloodRequestResponseDto MapResponseToDto(
            BloodRequestResponse response)
        {
            return new BloodRequestResponseDto
            {
                Id =
                    response.Id,

                BloodRequestId =
                    response.BloodRequestId,

                DonorId =
                    response.DonorId,

                DonorName =
                    response.DonorName,

                DonorPhone =
                    response.DonorPhone,

                BloodType =
                    response.BloodType,

                Status =
                    response.Status,

                CreatedAt =
                    DateTime.SpecifyKind(
                        response.CreatedAt,
                        DateTimeKind.Utc),

                UpdatedAt =
                    response.UpdatedAt.HasValue
                        ? DateTime.SpecifyKind(
                            response.UpdatedAt.Value,
                            DateTimeKind.Utc)
                        : null
            };
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

                // SQL Server can return DateTime values with Kind=Unspecified.
                // These values were originally stored as UTC, so explicitly mark
                // them as UTC before ASP.NET serializes them to JSON. This makes
                // the API return a timezone-aware value (with Z) to React Native.
                CreatedAt =
                DateTime.SpecifyKind(
                    request.CreatedAt,
                    DateTimeKind.Utc),

                UpdatedAt =
                request.UpdatedAt.HasValue
                    ? DateTime.SpecifyKind(
                        request.UpdatedAt.Value,
                        DateTimeKind.Utc)
                    : null,

                ExpireAt =
                request.ExpireAt.HasValue
                    ? DateTime.SpecifyKind(
                        request.ExpireAt.Value,
                        DateTimeKind.Utc)
                    : null
            };
        }
    }
}