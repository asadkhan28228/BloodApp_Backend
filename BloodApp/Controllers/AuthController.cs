
using System.Security.Claims;
using BloodDonationAPI.BLL.DTOs.Auth;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ==========================================
        // REGISTER
        // POST: api/auth/register
        // ==========================================

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result =
                await _authService.RegisterAsync(dto);

            if (result == null)
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            return Ok(new
            {
                message = result.RequiresEmailVerification
                    ? "Registration successful. Please verify your email."
                    : "Registration successful.",
                data = result
            });
        }

        // ==========================================
        // LOGIN
        // POST: api/auth/login
        // ==========================================

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result =
                await _authService.LoginAsync(dto);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid credentials, inactive account, " +
                        "or email not verified."
                });
            }

            return Ok(new
            {
                message = "Login successful.",
                data = result
            });
        }

        // ==========================================
        // VERIFY EMAIL - NEW
        // GET: api/auth/verify-email?token=...
        // ==========================================

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(
            [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new
                {
                    message = "Verification token is required."
                });
            }

            var verified =
                await _authService.VerifyEmailAsync(token);

            if (!verified)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid, expired, or already used " +
                        "verification link."
                });
            }

            return Ok(new
            {
                message =
                    "Email verified successfully. " +
                    "You can now log in."
            });
        }

        // ==========================================
        // RESEND VERIFICATION - NEW
        // POST: api/auth/resend-verification
        // ==========================================

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification(
            [FromBody] ResendVerificationRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    message = "Email is required."
                });
            }

            await _authService.ResendVerificationEmailAsync(
                request.Email);

            return Ok(new
            {
                message =
                    "If this email is registered and not verified, " +
                    "a new verification link will be sent."
            });
        }

        // ==========================================
        // PROFILE
        // GET: api/auth/profile
        // ==========================================

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(
                    userIdClaim,
                    out var userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var result =
                await _authService.GetProfileAsync(userId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            return Ok(new
            {
                message = "Profile retrieved successfully.",
                data = result
            });
        }

        // ==========================================
        // FORGOT PASSWORD
        // POST: api/auth/forgot-password
        // ==========================================

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _authService.ForgotPasswordAsync(dto);

            return Ok(new
            {
                message =
                    "If an account exists with this email, " +
                    "password reset instructions will be sent."
            });
        }

        // ==========================================
        // RESET PASSWORD
        // POST: api/auth/reset-password
        // ==========================================

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result =
                await _authService.ResetPasswordAsync(dto);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Invalid or expired reset token."
                });
            }

            return Ok(new
            {
                message = "Password reset successfully."
            });
        }

        // ==========================================
        // REFRESH TOKEN
        // POST: api/auth/refresh-token
        // ==========================================

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(
            [FromBody] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new
                {
                    message = "Refresh token is required."
                });
            }

            var result =
                await _authService.RefreshTokenAsync(token);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid refresh token."
                });
            }

            return Ok(new
            {
                message = "Token refreshed successfully.",
                data = result
            });
        }

        // ==========================================
        // LOGOUT
        // POST: api/auth/logout
        // ==========================================

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(
            [FromBody] string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new
                {
                    message = "Refresh token is required."
                });
            }

            var result =
                await _authService.LogoutAsync(refreshToken);

            if (!result)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid or already revoked refresh token."
                });
            }

            return Ok(new
            {
                message = "Logout successful."
            });
        }

        // ==========================================
        // RESEND VERIFICATION REQUEST DTO
        // ==========================================

        public class ResendVerificationRequest
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}
