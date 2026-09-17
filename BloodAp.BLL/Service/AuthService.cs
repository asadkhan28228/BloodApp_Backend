using System.Security.Cryptography;
using BloodDonationAPI.BLL.DTOs.Auth;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using BloodDonationAPI.DAL.Repositories;

namespace BloodDonationAPI.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
        }

        // =========================================================
        // REGISTER
        // =========================================================

        public async Task<AuthResponseDto?> RegisterAsync(
            RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var emailExists =
                await _userRepository.EmailExistsAsync(email);

            if (emailExists)
            {
                return null;
            }

            var passwordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName.Trim(),
                Email = email,
                PhoneNumber = dto.PhoneNumber.Trim(),
                PasswordHash = passwordHash,
                BloodType = dto.BloodType.Trim(),
                Gender = dto.Gender?.Trim(),
                Location = dto.Location?.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Role = "user",
                IsActive = true,
                AvailableToDonate = true,
                DonationsCount = 0,
                AvatarUrl = null,
                PushToken = null,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser =
                await _userRepository.AddAsync(user);

            return await CreateAuthResponseAsync(createdUser);
        }

        // =========================================================
        // LOGIN
        // =========================================================

        public async Task<AuthResponseDto?> LoginAsync(
            LoginDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user =
                await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            var passwordValid =
                BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.PasswordHash);

            if (!passwordValid)
            {
                return null;
            }

            return await CreateAuthResponseAsync(user);
        }

        // =========================================================
        // GET PROFILE
        // =========================================================

        public async Task<UserProfileDto?> GetProfileAsync(
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
        // FORGOT PASSWORD
        // =========================================================

        public async Task<bool> ForgotPasswordAsync(
            ForgotPasswordDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user =
                await _userRepository.GetByEmailAsync(email);

            // Do not reveal whether the email exists.
            if (user == null)
            {
                return true;
            }

            await _passwordResetTokenRepository
                .InvalidateUserTokensAsync(user.Id);

            var tokenBytes =
                RandomNumberGenerator.GetBytes(32);

            var token =
                Convert.ToBase64String(tokenBytes);

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetTokenRepository
                .AddAsync(resetToken);

            // Gmail email service will be connected later.
            Console.WriteLine(
                $"PASSWORD RESET TOKEN for {email}: {token}");

            return true;
        }

        // =========================================================
        // RESET PASSWORD
        // =========================================================

        public async Task<bool> ResetPasswordAsync(
            ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
            {
                return false;
            }

            var resetToken =
                await _passwordResetTokenRepository
                    .GetByTokenAsync(dto.Token.Trim());

            if (resetToken == null)
            {
                return false;
            }

            if (resetToken.IsUsed)
            {
                return false;
            }

            if (resetToken.ExpiresAt <= DateTime.UtcNow)
            {
                return false;
            }

            var user =
                await _userRepository
                    .GetByIdAsync(resetToken.UserId);

            if (user == null)
            {
                return false;
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    dto.NewPassword);

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            resetToken.IsUsed = true;

            await _passwordResetTokenRepository
                .UpdateAsync(resetToken);

            return true;
        }

        // =========================================================
        // REFRESH TOKEN
        // =========================================================

        public async Task<AuthResponseDto?> RefreshTokenAsync(
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var refreshToken =
                await _refreshTokenRepository
                    .GetByTokenAsync(token.Trim());

            if (refreshToken == null)
            {
                return null;
            }

            // Token has already been revoked.
            if (refreshToken.IsRevoked)
            {
                return null;
            }

            // Token has expired.
            if (refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return null;
            }

            var user = refreshToken.User;

            if (user == null)
            {
                return null;
            }

            // User account must still be active.
            if (!user.IsActive)
            {
                return null;
            }

            // Revoke old refresh token.
            await _refreshTokenRepository
                .RevokeAsync(refreshToken);

            // Generate a new access token + refresh token.
            return await CreateAuthResponseAsync(user);
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        public async Task<bool> LogoutAsync(
            string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            var token =
                await _refreshTokenRepository
                    .GetByTokenAsync(refreshToken.Trim());

            if (token == null)
            {
                return false;
            }

            // Already revoked
            if (token.IsRevoked)
            {
                return false;
            }

            // Revoke refresh token
            await _refreshTokenRepository
                .RevokeAsync(token);

            return true;
        }

        // =========================================================
        // CREATE AUTH RESPONSE
        // =========================================================

        private async Task<AuthResponseDto> CreateAuthResponseAsync(
            User user)
        {
            // Generate JWT access token.
            var accessToken =
                _jwtService.GenerateToken(user);

            var accessTokenExpiresAt =
                _jwtService.GetExpiration();

            // Generate secure refresh token.
            var refreshTokenBytes =
                RandomNumberGenerator.GetBytes(64);

            var refreshTokenValue =
                Convert.ToBase64String(refreshTokenBytes);

            // Refresh token lifetime: 7 days.
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),

                UserId = user.Id,

                Token = refreshTokenValue,

                ExpiresAt =
                    DateTime.UtcNow.AddDays(7),

                IsRevoked = false,

                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository
                .AddAsync(refreshToken);

            return new AuthResponseDto
            {
                Token = accessToken,

                RefreshToken = refreshTokenValue,

                ExpiresAt = accessTokenExpiresAt,

                User = MapUserToProfile(user)
            };
        }

        // =========================================================
        // MAP USER TO PROFILE DTO
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