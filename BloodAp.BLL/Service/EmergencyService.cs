using BloodDonationAPI.BLL.DTOs.Emergency;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class EmergencyService : IEmergencyService
    {
        private readonly IEmergencyRequestRepository _emergencyRepository;
        private readonly IEmergencyResponseRepository _responseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBloodInventoryRepository _inventoryRepository;
        private readonly IBloodRequestRepository _bloodRequestRepository;
        private readonly ICommunityNotificationRepository _communityNotificationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;

        public EmergencyService(
            IEmergencyRequestRepository emergencyRepository,
            IEmergencyResponseRepository responseRepository,
            IUserRepository userRepository,
            IBloodInventoryRepository inventoryRepository,
            IBloodRequestRepository bloodRequestRepository,
            ICommunityNotificationRepository communityNotificationRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher)
        {
            _emergencyRepository = emergencyRepository;
            _responseRepository = responseRepository;
            _userRepository = userRepository;
            _inventoryRepository = inventoryRepository;
            _bloodRequestRepository = bloodRequestRepository;
            _communityNotificationRepository =
                communityNotificationRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<EmergencyRequestDto?> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            string currentUserRole)
        {
            var emergency =
                await _emergencyRepository.GetByIdAsync(id);

            if (emergency == null)
                return null;

            return MapEmergencyToDto(emergency);
        }

        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<List<EmergencyRequestDto>> GetAllAsync()
        {
            var emergencies =
                await _emergencyRepository.GetAllAsync();

            return emergencies
                .Select(MapEmergencyToDto)
                .ToList();
        }

        // =========================================================
        // GET ACTIVE
        // =========================================================

        public async Task<List<EmergencyRequestDto>> GetActiveAsync()
        {
            var emergencies =
                await _emergencyRepository.GetActiveAsync();

            return emergencies
                .Select(MapEmergencyToDto)
                .ToList();
        }

        // =========================================================
        // GET MY REQUESTS
        // =========================================================

        public async Task<List<EmergencyRequestDto>>
            GetMyRequestsAsync(Guid currentUserId)
        {
            var emergencies =
                await _emergencyRepository
                    .GetByReporterAsync(currentUserId);

            return emergencies
                .Select(MapEmergencyToDto)
                .ToList();
        }

        // =========================================================
        // CREATE EMERGENCY
        // =========================================================

        public async Task<EmergencyRequestDto?> CreateAsync(
            CreateEmergencyRequestDto dto,
            Guid currentUserId)
        {
            if (dto == null)
                return null;

            var user =
                await _userRepository.GetByIdAsync(currentUserId);

            if (user == null)
                return null;

            if (!user.IsActive)
                return null;

            var category =
                dto.Category.Trim();

            var priority =
                dto.Priority.Trim();

            var bloodType =
                dto.BloodType.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(category))
                return null;

            if (string.IsNullOrWhiteSpace(bloodType))
                return null;

            var allowedPriorities = new[]
            {
                "Critical",
                "High",
                "Medium"
            };

            if (!allowedPriorities.Contains(
                    priority,
                    StringComparer.OrdinalIgnoreCase))
            {
                priority = "High";
            }

            // -----------------------------------------------------
            // Try Blood Bank Inventory Reserve
            // -----------------------------------------------------

            if (dto.UnitsNeeded <= 0) return null;

            int reservedUnits = 0;
            if (bloodType != "ANY")
            {
                reservedUnits = await _inventoryRepository
                    .ReserveAvailableUnitsAsync(bloodType, dto.UnitsNeeded);
            }

            int remainingUnits = dto.UnitsNeeded - reservedUnits;
            bool reserved = remainingUnits == 0;
            var bloodSource = reservedUnits > 0
                ? "Blood Bank Reserve"
                : "Community Donors";

            // -----------------------------------------------------
            // Emergency information
            // -----------------------------------------------------

            var now = DateTime.UtcNow;

            var expireAt =
                now.AddDays(2);

            var title =
                $"{priority.ToUpper()}: {category} Alert! 🚨";

            string message;

            if (reserved)
            {
                message =
                    $"Emergency ({category}) reported at " +
                    $"{dto.Location}. " +
                    $"{dto.UnitsNeeded} unit(s) of " +
                    $"{bloodType} blood reserved from the blood bank.";
            }
            else
            {
                message =
                    $"Emergency ({category}) reported. " +
                    $"{remainingUnits} unit(s) of {bloodType} blood needed " +
                    $"from donors at {dto.Location}! " +
                    $"{reservedUnits} unit(s) reserved from blood bank.";
            }

            var emergency = new EmergencyRequest
            {
                Id = Guid.NewGuid(),

                Category = category,

                Priority = priority,

                BloodType = bloodType,

                UnitsNeeded = dto.UnitsNeeded,

                Status = "Active",

                BloodSource = bloodSource,

                ReservedUnits = reservedUnits,

                Location = dto.Location.Trim(),

                Latitude = dto.Latitude,

                Longitude = dto.Longitude,

                PhoneNumber = dto.PhoneNumber.Trim(),

                Details = dto.Details?.Trim(),

                ReporterUid = currentUserId,

                ReporterName = user.FullName,

                Title = title,

                Message = message,

                Unread = true,

                Icon = category.Equals(
                    "Fire",
                    StringComparison.OrdinalIgnoreCase)
                    ? "flame"
                    : "alert-circle",

                Color = GetPriorityColor(priority),

                ExpireAt = expireAt,

                CreatedAt = now,

                UpdatedAt = now,

                RespondingDonorsCount = 0
            };

            try
            {
                // -------------------------------------------------
                // Save Emergency
                // -------------------------------------------------

                emergency =
                    await _emergencyRepository.AddAsync(
                        emergency);

                // -------------------------------------------------
                // Create linked Blood Request
                // -------------------------------------------------

                var bloodRequest = new BloodRequest
                {
                    Id = Guid.NewGuid(),

                    PatientName =
                        $"Emergency ({category})",

                    BloodType = bloodType,

                    Urgency = priority,

                    UnitsNeeded = dto.UnitsNeeded,

                    HospitalName =
                        dto.Location.Trim(),

                    Location =
                        dto.Location.Trim(),

                    Latitude = dto.Latitude,

                    Longitude = dto.Longitude,

                    ContactPhone =
                        dto.PhoneNumber.Trim(),

                    AdditionalNotes =
                        $"Emergency Alert. Details: " +
                        $"{dto.Details ?? "No details provided."}",

                    Status = reserved
                        ? "Reserved"
                        : "Pending",

                    RequestType = "Emergency",

                    BloodSource = bloodSource,

                    ReporterUid = currentUserId,

                    ReporterName = user.FullName,

                    EmergencyId = emergency.Id,

                    CreatedAt = now,

                    ExpireAt = expireAt
                };

                await _bloodRequestRepository.AddAsync(
                    bloodRequest);

                // Notify donors only when the blood bank could not reserve all requested units.
                // Fully reserved emergencies remain saved, but generate no community alert.
                if (!reserved)
                {
                    // -------------------------------------------------
                    // Community Notification
                    // -------------------------------------------------

                    var notification =
                        new CommunityNotification
                        {
                            Id = Guid.NewGuid(),

                            Type = "emergency",

                            Title = title,

                            Message = message,

                            Unread = true,

                            Icon =
                                category.Equals(
                                    "Fire",
                                    StringComparison.OrdinalIgnoreCase)
                                    ? "flame"
                                    : "alert-circle",

                            Color =
                                GetPriorityColor(priority),

                            ReporterUid =
                                currentUserId,

                            ReporterName =
                                user.FullName,

                            BloodRequestId =
                                bloodRequest.Id,

                            EmergencyRequestId =
                                emergency.Id,

                            Status = "Active",

                            Category = category,

                            Priority = priority,

                            BloodType = bloodType,

                            UnitsNeeded =
                                remainingUnits,

                            BloodSource =
                                bloodSource,

                            ReservedUnits =
                                reservedUnits,

                            Location =
                                dto.Location.Trim(),

                            Latitude =
                                dto.Latitude,

                            Longitude =
                                dto.Longitude,

                            PhoneNumber =
                                dto.PhoneNumber.Trim(),

                            Details =
                                dto.Details?.Trim(),

                            ExpireAt =
                                expireAt,

                            CreatedAt =
                                now,

                            UpdatedAt =
                                now
                        };

                    await _communityNotificationRepository
                        .AddAsync(notification);

                    // -------------------------------------------------
                    // Real-time SignalR notification
                    // -------------------------------------------------

                    await _notificationPublisher
                        .PublishCommunityNotificationAsync(
                            notification);

                }

                return MapEmergencyToDto(
                    emergency);
            }
            catch
            {
                // Return reserved blood if creation fails
                if (reservedUnits > 0)
                {
                    await _inventoryRepository
                        .ReleaseUnitsAsync(
                            bloodType,
                            reservedUnits);
                }

                throw;
            }
        }

        // =========================================================
        // CANCEL EMERGENCY
        // Reporter OR Admin
        // =========================================================

        public async Task<bool> CancelAsync(
            Guid id,
            Guid currentUserId,
            string currentUserRole)
        {
            var emergency =
                await _emergencyRepository.GetByIdAsync(id);

            if (emergency == null)
                return false;

            bool isAdmin =
                IsAdminRole(currentUserRole);

            if (!isAdmin &&
                emergency.ReporterUid != currentUserId)
            {
                return false;
            }

            if (emergency.Status != "Active")
                return false;

            // -----------------------------------------------------
            // Release reserved blood
            // -----------------------------------------------------

            if (emergency.ReservedUnits > 0 &&
                emergency.BloodSource ==
                    "Blood Bank Reserve" &&
                emergency.BloodType != "ANY")
            {
                await _inventoryRepository
                    .ReleaseUnitsAsync(
                        emergency.BloodType,
                        emergency.ReservedUnits);

                emergency.ReservedUnits = 0;
            }

            // -----------------------------------------------------
            // Delete community notifications
            // -----------------------------------------------------

            var communityNotifications =
                await _communityNotificationRepository
                    .GetByEmergencyRequestAsync(
                        emergency.Id);

            foreach (var notification
                     in communityNotifications)
            {
                await _communityNotificationRepository
                    .DeleteAsync(notification);
            }

            // -----------------------------------------------------
            // Delete linked blood request
            // -----------------------------------------------------

            var bloodRequest =
                await _bloodRequestRepository
                    .GetByEmergencyIdAsync(
                        emergency.Id);

            if (bloodRequest != null)
            {
                await _bloodRequestRepository
                    .DeleteAsync(bloodRequest);
            }

            // -----------------------------------------------------
            // Delete emergency
            // -----------------------------------------------------

            await _emergencyRepository
                .DeleteAsync(emergency);

            return true;
        }

        // =========================================================
        // RESOLVE EMERGENCY
        // Reporter OR Admin
        // =========================================================

        public async Task<bool> ResolveAsync(
            Guid id,
            Guid currentUserId,
            string currentUserRole)
        {
            var emergency =
                await _emergencyRepository.GetByIdAsync(id);

            if (emergency == null)
                return false;

            bool isAdmin =
                IsAdminRole(currentUserRole);

            if (!isAdmin &&
                emergency.ReporterUid != currentUserId)
            {
                return false;
            }

            if (emergency.Status != "Active")
                return false;

            // Reserved blood is consumed on resolution, not returned to stock.
            // Cancellation still releases unused reserved units.

            var now = DateTime.UtcNow;

            emergency.Status = "Resolved";

            emergency.ResolvedAt = now;

            emergency.UpdatedAt = now;

            await _emergencyRepository
                .UpdateAsync(emergency);

            // -----------------------------------------------------
            // Mark linked Blood Request as Completed
            // -----------------------------------------------------

            var bloodRequest =
                await _bloodRequestRepository
                    .GetByEmergencyIdAsync(
                        emergency.Id);

            if (bloodRequest != null)
            {
                bloodRequest.Status = "Completed";
                bloodRequest.UpdatedAt = now;

                await _bloodRequestRepository
                    .UpdateAsync(bloodRequest);
            }

            // -----------------------------------------------------
            // Notify responding donors
            // -----------------------------------------------------

            var responses =
                await _responseRepository
                    .GetByEmergencyAsync(
                        emergency.Id);

            foreach (var response in responses)
            {
                var privateNotification =
                    new Notification
                    {
                        Id = Guid.NewGuid(),

                        UserId =
                            response.DonorId,

                        Type =
                            "emergency_resolved",

                        Title =
                            "Emergency Resolved ✅",

                        Message =
                            $"The emergency reported by " +
                            $"{emergency.ReporterName} " +
                            $"has been resolved.",

                        Icon =
                            "checkmark-circle",

                        Color =
                            "#16A34A",

                        Unread = true,

                        TargetScreen =
                            "emergency",

                        ReferenceId =
                            emergency.Id.ToString(),

                        CreatedAt = now
                    };

                await _notificationRepository
                    .AddAsync(
                        privateNotification);

                await _notificationPublisher
                    .PublishUserNotificationAsync(
                        privateNotification.UserId,
                        privateNotification);
            }

            // -----------------------------------------------------
            // Delete emergency community notifications
            // -----------------------------------------------------

            var communityNotifications =
                await _communityNotificationRepository
                    .GetByEmergencyRequestAsync(
                        emergency.Id);

            foreach (var notification
                     in communityNotifications)
            {
                await _communityNotificationRepository
                    .DeleteAsync(notification);
            }

            // -----------------------------------------------------
            // Delete linked Blood Request
            // -----------------------------------------------------

            if (bloodRequest != null)
            {
                await _bloodRequestRepository
                    .DeleteAsync(bloodRequest);
            }

            return true;
        }

        // =========================================================
        // DONOR RESPOND
        // "I Can Help"
        // =========================================================

        public async Task<EmergencyResponseDto?>
            RespondAsync(
                Guid emergencyId,
                Guid donorId)
        {
            var emergency =
                await _emergencyRepository
                    .GetByIdAsync(emergencyId);

            if (emergency == null)
                return null;

            if (emergency.Status != "Active")
                return null;

            // Do not accept new volunteers once a journey is claimed.
            var currentResponses = await _responseRepository.GetByEmergencyAsync(emergencyId);
            if (currentResponses.Any(x =>
                string.Equals(x.Status, "On The Way", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Status, "Arrived", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Status, "Completed", StringComparison.OrdinalIgnoreCase)))
                return null;

            if (emergency.ExpireAt.HasValue &&
                emergency.ExpireAt.Value <=
                    DateTime.UtcNow)
            {
                return null;
            }

            var donor =
                await _userRepository
                    .GetByIdAsync(donorId);

            if (donor == null)
                return null;

            if (!donor.IsActive)
                return null;

            // Reporter cannot respond to own emergency
            if (emergency.ReporterUid == donorId)
                return null;

            // Donor must be available
            if (donor.AvailableToDonate == false)
                return null;

            // -----------------------------------------------------
            // Prevent duplicate response
            // -----------------------------------------------------

            var existing =
                await _responseRepository
                    .GetByEmergencyAndDonorAsync(
                        emergencyId,
                        donorId);

            if (existing != null)
            {
                return MapResponseToDto(
                    existing);
            }

            // -----------------------------------------------------
            // Frontend-compatible blood matching
            //
            // Specific blood type = exact same type
            // ANY = any donor
            // -----------------------------------------------------

            if (!IsCompatibleBloodType(
                    donor.BloodType,
                    emergency.BloodType))
            {
                return null;
            }

            var response =
                new EmergencyResponse
                {
                    Id = Guid.NewGuid(),

                    EmergencyRequestId =
                        emergencyId,

                    DonorId =
                        donorId,

                    DonorName =
                        donor.FullName,

                    DonorPhone =
                        donor.PhoneNumber,

                    BloodType =
                        donor.BloodType,

                    Status =
                        "Accepted",

                    Latitude =
                        donor.Latitude,

                    Longitude =
                        donor.Longitude,

                    RespondedAt =
                        DateTime.UtcNow,

                    UpdatedAt =
                        DateTime.UtcNow
                };

            response =
                await _responseRepository
                    .AddAsync(response);

            // -----------------------------------------------------
            // Update responder count
            // -----------------------------------------------------

            emergency.RespondingDonorsCount++;

            emergency.UpdatedAt =
                DateTime.UtcNow;

            await _emergencyRepository
                .UpdateAsync(emergency);

            // -----------------------------------------------------
            // Notify emergency reporter
            // -----------------------------------------------------

            await CreateEmergencyResponseNotificationAsync(
                emergency,
                response);

            return MapResponseToDto(
                response);
        }

        // =========================================================
        // GET EMERGENCY RESPONSES
        // =========================================================

        public async Task<List<EmergencyResponseDto>>
            GetResponsesAsync(Guid emergencyId)
        {
            var responses =
                await _responseRepository
                    .GetByEmergencyAsync(
                        emergencyId);

            var result = new List<EmergencyResponseDto>();

            foreach (var response in responses)
            {
                var dto = MapResponseToDto(response);

                // Always read the latest donor profile data so phone/location
                // are available even for older EmergencyResponse rows.
                var donor = await _userRepository.GetByIdAsync(response.DonorId);

                if (donor != null)
                {
                    dto.DonorPhone = donor.PhoneNumber;
                    dto.DonorLocation = donor.Location;
                    dto.Latitude = donor.Latitude;
                    dto.Longitude = donor.Longitude;
                }

                result.Add(dto);
            }

            return result;
        }

        // =========================================================
        // GET MY RESPONSES
        // =========================================================

        public async Task<List<EmergencyResponseDto>>
            GetMyResponsesAsync(Guid donorId)
        {
            var responses =
                await _responseRepository
                    .GetByDonorAsync(donorId);

            return responses
                .Select(MapResponseToDto)
                .ToList();
        }

        // =========================================================
        // UPDATE RESPONSE STATUS
        // =========================================================

        public async Task<bool>
            UpdateResponseStatusAsync(
                Guid responseId,
                Guid donorId,
                string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var normalizedStatus = status.Trim();

            var response = await _responseRepository.GetByIdAsync(responseId);
            if (response == null || response.DonorId != donorId)
                return false;

            var emergency = await _emergencyRepository
                .GetByIdAsync(response.EmergencyRequestId);

            if (emergency == null ||
                !string.Equals(emergency.Status, "Active", StringComparison.OrdinalIgnoreCase) ||
                (emergency.ExpireAt.HasValue && emergency.ExpireAt.Value <= DateTime.UtcNow))
                return false;

            // START JOURNEY: database transaction ensures that only
            // one accepted donor can claim an emergency.
            if (normalizedStatus.Equals("On The Way", StringComparison.OrdinalIgnoreCase))
            {
                // Do not allow an existing journey to be restarted.
                if (!string.Equals(response.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                    return false;

                var started = await _responseRepository
                    .TryStartJourneyAsync(responseId, donorId);

                if (!started)
                    return false;

                // ExecuteUpdateAsync bypasses EF's change tracker.
                // Get a fresh no-tracking copy for the notification.
                var donorResponses = await _responseRepository.GetByDonorAsync(donorId);
                var updatedResponse = donorResponses.FirstOrDefault(x => x.Id == responseId);

                if (updatedResponse != null)
                {
                    await CreateEmergencyResponseNotificationAsync(emergency, updatedResponse);
                }

                return true;
            }

            // Accepted is set when a donor first responds. It must
            // not be used to reset an assigned journey.
            if (normalizedStatus.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
                return false;

            // Only the assigned donor may advance the journey.
            // A completed response is terminal and keeps the claim.
            var responses = await _responseRepository
                .GetByEmergencyAsync(response.EmergencyRequestId);

            var otherClaim = responses.Any(x =>
                x.Id != responseId &&
                (string.Equals(x.Status, "On The Way", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.Status, "Arrived", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.Status, "Completed", StringComparison.OrdinalIgnoreCase)));

            if (otherClaim)
                return false;

            var now = DateTime.UtcNow;

            if (normalizedStatus.Equals("Arrived", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(response.Status, "On The Way", StringComparison.OrdinalIgnoreCase))
                    return false;

                response.Status = "Arrived";
                response.ArrivedAt ??= now;
            }
            else if (normalizedStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(response.Status, "Arrived", StringComparison.OrdinalIgnoreCase))
                    return false;

                response.Status = "Completed";
                response.CompletedAt ??= now;
            }
            else
            {
                return false;
            }

            response.UpdatedAt = now;
            await _responseRepository.UpdateAsync(response);
            await CreateEmergencyResponseNotificationAsync(emergency, response);
            return true;
        }

        // =========================================================
        // COMPLETE RESPONSE
        // =========================================================

        public async Task<bool>
            CompleteResponseAsync(
                Guid responseId,
                Guid donorId)
        {
            return await UpdateResponseStatusAsync(
                responseId,
                donorId,
                "Completed");
        }

        // =========================================================
        // EMERGENCY RESPONSE NOTIFICATION
        // =========================================================

        private async Task
            CreateEmergencyResponseNotificationAsync(
                EmergencyRequest emergency,
                EmergencyResponse response)
        {
            var now = DateTime.UtcNow;

            string title;
            string message;
            string icon;
            string color;

            switch (response.Status)
            {
                case "Accepted":

                    title =
                        "Emergency Response Received 🚑";

                    message =
                        $"{response.DonorName} accepted " +
                        "your emergency and is ready to help.";

                    icon = "medical";
                    color = "#DC2626";

                    break;

                case "On The Way":

                    title =
                        "Donor Is On The Way 🚗";

                    message =
                        $"{response.DonorName} is on the way " +
                        "to help with your emergency.";

                    icon = "car";
                    color = "#EA580C";

                    break;

                case "Arrived":

                    title =
                        "Donor Has Arrived 📍";

                    message =
                        $"{response.DonorName} has arrived " +
                        "to help with your emergency.";

                    icon = "location";
                    color = "#2563EB";

                    break;

                case "Completed":

                    title =
                        "Emergency Response Completed ✅";

                    message =
                        $"{response.DonorName} completed " +
                        "the emergency response.";

                    icon = "checkmark-circle";
                    color = "#16A34A";

                    break;

                default:

                    title =
                        "Emergency Response Update 🚑";

                    message =
                        $"{response.DonorName} updated " +
                        $"their response to {response.Status}.";

                    icon = "notifications";
                    color = "#DC2626";

                    break;
            }

            // -----------------------------------------------------
            // Private notification
            // -----------------------------------------------------

            var privateNotification =
                new Notification
                {
                    Id = Guid.NewGuid(),

                    UserId =
                        emergency.ReporterUid,

                    Type =
                        "emergency_response",

                    Title =
                        title,

                    Message =
                        message,

                    Icon =
                        icon,

                    Color =
                        color,

                    Unread = true,

                    TargetScreen =
                        "emergency",

                    ReferenceId =
                        emergency.Id.ToString(),

                    CreatedAt =
                        now
                };

            await _notificationRepository
                .AddAsync(
                    privateNotification);

            await _notificationPublisher
                .PublishUserNotificationAsync(
                    privateNotification.UserId,
                    privateNotification);

            // Emergency response updates are private.
            // They are stored in Notifications and sent only to the reporter
            // through the reporter's SignalR user group.
        }

        // =========================================================
        // MAPPING
        // =========================================================

        private static EmergencyRequestDto
            MapEmergencyToDto(
                EmergencyRequest emergency)
        {
            return new EmergencyRequestDto
            {
                Id =
                    emergency.Id,

                Category =
                    emergency.Category,

                Priority =
                    emergency.Priority,

                BloodType =
                    emergency.BloodType,

                UnitsNeeded =
                    emergency.UnitsNeeded,

                Status =
                    emergency.Status,

                BloodSource =
                    emergency.BloodSource,

                ReservedUnits =
                    emergency.ReservedUnits,

                Location =
                    emergency.Location,

                Latitude =
                    emergency.Latitude,

                Longitude =
                    emergency.Longitude,

                PhoneNumber =
                    emergency.PhoneNumber,

                Details =
                    emergency.Details,

                ReporterUid =
                    emergency.ReporterUid,

                ReporterName =
                    emergency.ReporterName,

                Title =
                    emergency.Title,

                Message =
                    emergency.Message,

                Unread =
                    emergency.Unread,

                Icon =
                    emergency.Icon,

                Color =
                    emergency.Color,

                ExpireAt =
                    emergency.ExpireAt,

                CreatedAt =
                    emergency.CreatedAt,

                UpdatedAt =
                    emergency.UpdatedAt,

                ResolvedAt =
                    emergency.ResolvedAt,

                RespondingDonorsCount =
                    emergency.RespondingDonorsCount
            };
        }

        private static EmergencyResponseDto
            MapResponseToDto(
                EmergencyResponse response)
        {
            return new EmergencyResponseDto
            {
                Id =
                    response.Id,

                EmergencyRequestId =
                    response.EmergencyRequestId,

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

                Latitude =
                    response.Latitude,

                Longitude =
                    response.Longitude,

                RespondedAt =
                    response.RespondedAt,

                OnTheWayAt =
                    response.OnTheWayAt,

                ArrivedAt =
                    response.ArrivedAt,

                CompletedAt =
                    response.CompletedAt,

                UpdatedAt =
                    response.UpdatedAt
            };
        }

        // =========================================================
        // ADMIN CHECK
        // =========================================================

        private static bool IsAdminRole(
            string role)
        {
            return role.Equals(
                       "admin",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   role.Equals(
                       "bloodBankAdmin",
                       StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // PRIORITY COLOR
        // Frontend colors
        // =========================================================

        private static string GetPriorityColor(
            string priority)
        {
            return priority.ToLower() switch
            {
                "critical" => "#DC2626",
                "high" => "#EA580C",
                "medium" => "#CA8A04",
                _ => "#DC2626"
            };
        }

        // =========================================================
        // BLOOD COMPATIBILITY
        //
        // Frontend emergency donor matching:
        // same blood type OR emergency type = ANY
        // =========================================================

        private static bool IsCompatibleBloodType(
            string? donorBloodType,
            string emergencyBloodType)
        {
            if (string.IsNullOrWhiteSpace(
                    donorBloodType))
            {
                return false;
            }

            donorBloodType =
                donorBloodType.Trim().ToUpper();

            emergencyBloodType =
                emergencyBloodType.Trim().ToUpper();

            if (emergencyBloodType == "ANY")
                return true;

            return donorBloodType ==
                   emergencyBloodType;
        }
    }
}