using BloodDonationAPI.BLL.DTOs.Auth;
using BloodDonationAPI.BLL.DTOs.User;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(
            IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // =========================================================
        // GET USER BY ID
        // =========================================================

        public async Task<UserProfileDto?> GetByIdAsync(
            Guid userId)
        {
            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            return MapUserToProfile(user);
        }

        // =========================================================
        // GET ALL USERS
        // =========================================================

        public async Task<List<UserProfileDto>> GetAllAsync()
        {
            var users =
                await _userRepository.GetAllAsync();

            return users
                .Select(MapUserToProfile)
                .ToList();
        }
        // =========================================================
        // Get DonorAsync
        // =========================================================

        public async Task<List<DonorDto>> GetDonorsAsync(
    string? bloodType = null,
    bool? availableToDonate = null,
    double? latitude = null,
    double? longitude = null)
        {
            var donors = await _userRepository.GetDonorsAsync(
                bloodType,
                availableToDonate);

            var result = donors
                .Select(user => new DonorDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    BloodType = user.BloodType,
                    Gender = user.Gender,
                    Location = user.Location,
                    Latitude = user.Latitude,
                    Longitude = user.Longitude,
                    AvailableToDonate = user.AvailableToDonate,
                    LastDonationDate = user.LastDonationDate,
                    DonationsCount = user.DonationsCount,
                    AvatarUrl = user.AvatarUrl,
                    IsActive = user.IsActive,

                    DistanceKm =
                        latitude.HasValue &&
                        longitude.HasValue &&
                        user.Latitude.HasValue &&
                        user.Longitude.HasValue
                            ? CalculateDistance(
                                latitude.Value,
                                longitude.Value,
                                user.Latitude.Value,
                                user.Longitude.Value)
                            : null
                })
                .OrderBy(x => x.DistanceKm ?? double.MaxValue)
                .ToList();

            return result;
        }
        // =========================================================
        // UPDATE PROFILE
        // =========================================================

        public async Task<UserProfileDto?> UpdateProfileAsync(
            Guid userId,
            UpdateProfileDto dto)
        {
            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            user.FullName =
                dto.FullName.Trim();

            user.PhoneNumber =
                dto.PhoneNumber.Trim();

            user.BloodType =
                dto.BloodType.Trim();

            user.Gender =
                string.IsNullOrWhiteSpace(dto.Gender)
                    ? null
                    : dto.Gender.Trim();

            user.Location =
                string.IsNullOrWhiteSpace(dto.Location)
                    ? null
                    : dto.Location.Trim();

            user.Latitude =
                dto.Latitude;

            user.Longitude =
                dto.Longitude;

            user.AvailableToDonate =
                dto.AvailableToDonate;

            user.AvatarUrl =
                string.IsNullOrWhiteSpace(dto.AvatarUrl)
                    ? null
                    : dto.AvatarUrl.Trim();

            user.PushToken =
                string.IsNullOrWhiteSpace(dto.PushToken)
                    ? null
                    : dto.PushToken.Trim();

            user.UpdatedAt =
                DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return MapUserToProfile(user);
        }

        // =========================================================
        // UPDATE PUSH TOKEN
        // =========================================================

        public async Task<bool> UpdatePushTokenAsync(
            Guid userId,
            string? pushToken)
        {
            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.PushToken =
                string.IsNullOrWhiteSpace(pushToken)
                    ? null
                    : pushToken.Trim();

            user.UpdatedAt =
                DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return true;
        }

        // =========================================================
        // UPDATE DONATION AVAILABILITY
        // =========================================================

        public async Task<bool> UpdateAvailabilityAsync(
            Guid userId,
            bool availableToDonate)
        {
            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.AvailableToDonate =
                availableToDonate;

            user.UpdatedAt =
                DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return true;
        }
        // =========================================================
        // ACTIVATE USER
        // =========================================================

        public async Task<bool> ActivateUserAsync(
            Guid userId)
        {
            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.IsActive = true;

            user.UpdatedAt =
                DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return true;
        }
        // =========================================================
        // DEACTIVATE USER
        // =========================================================

        public async Task<bool> DeactivateUserAsync(
            Guid userId)
        {
            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.IsActive = false;

            user.AvailableToDonate = false;

            user.UpdatedAt =
                DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return true;
        }


        public async Task<bool> UpdateRoleAsync(Guid userId, string role)
        {
            var normalizedRole = role?.Trim();
            if (normalizedRole != "user" && normalizedRole != "admin" && normalizedRole != "bloodBankAdmin")
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.Role = normalizedRole;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        private static double CalculateDistance(
    double latitude1,
    double longitude1,
    double latitude2,
    double longitude2)
        {
            const double earthRadiusKm = 6371.0;

            var lat1 = DegreesToRadians(latitude1);
            var lat2 = DegreesToRadians(latitude2);

            var deltaLatitude =
                DegreesToRadians(latitude2 - latitude1);

            var deltaLongitude =
                DegreesToRadians(longitude2 - longitude1);

            var a =
                Math.Sin(deltaLatitude / 2) *
                Math.Sin(deltaLatitude / 2) +
                Math.Cos(lat1) *
                Math.Cos(lat2) *
                Math.Sin(deltaLongitude / 2) *
                Math.Sin(deltaLongitude / 2);

            var c =
                2 * Math.Atan2(
                    Math.Sqrt(a),
                    Math.Sqrt(1 - a));

            return Math.Round(
                earthRadiusKm * c,
                2);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
        // =========================================================
        // MAP USER → PROFILE DTO
        // =========================================================

        private UserProfileDto MapUserToProfile(
            User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,

                FullName = user.FullName,

                Email = user.Email,

                PhoneNumber = user.PhoneNumber,

                BloodType = user.BloodType,

                Location = user.Location,

                Latitude = user.Latitude,

                Longitude = user.Longitude,

                AvailableToDonate =
                    user.AvailableToDonate,

                LastDonationDate =
                    user.LastDonationDate,

                AvatarUrl =
                    user.AvatarUrl,

                Gender =
                    user.Gender,

                CreatedAt =
                    user.CreatedAt,

                Role =
                    user.Role,

                IsActive =
                    user.IsActive,

                DonationsCount =
                    user.DonationsCount,

                PushToken =
                    user.PushToken
            };
        }
    }
}