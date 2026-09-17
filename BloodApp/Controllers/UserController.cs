using System.Security.Claims;
using BloodDonationAPI.BLL.DTOs.User;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/user/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var result = await _userService.GetByIdAsync(userId.Value);

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

        // GET: api/user/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "User retrieved successfully.",
                data = result
            });
        }

        // GET: api/user/donors
        [HttpGet("donors")]
        public async Task<IActionResult> GetDonors(
    [FromQuery] string? bloodType = null,
    [FromQuery] bool? availableToDonate = null,
    [FromQuery] double? latitude = null,
    [FromQuery] double? longitude = null)
        {
            var result = await _userService.GetDonorsAsync(
                bloodType,
                availableToDonate,
                latitude,
                longitude);

            return Ok(new
            {
                message = "Donors retrieved successfully.",
                data = result
            });
        }

        // GET: api/user
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();

            return Ok(new
            {
                message = "Users retrieved successfully.",
                data = result
            });
        }

        // PUT: api/user/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var result = await _userService.UpdateProfileAsync(
                userId.Value,
                dto);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            return Ok(new
            {
                message = "Profile updated successfully.",
                data = result
            });
        }

        // PUT: api/user/push-token
        [HttpPut("push-token")]
        public async Task<IActionResult> UpdatePushToken(
            [FromBody] string? pushToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var result = await _userService.UpdatePushTokenAsync(
                userId.Value,
                pushToken);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "Push token updated successfully."
            });
        }

        // PUT: api/user/availability
        [HttpPut("availability")]
        public async Task<IActionResult> UpdateAvailability(
            [FromBody] bool availableToDonate)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var result = await _userService.UpdateAvailabilityAsync(
                userId.Value,
                availableToDonate);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "Donation availability updated successfully."
            });
        }

        // DELETE: api/user/account
        [HttpDelete("account")]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var result = await _userService.DeactivateUserAsync(
                userId.Value);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "Account deactivated successfully."
            });
        }

        [HttpPut("admin/{id:guid}/role")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] string role)
        {
            var result = await _userService.UpdateRoleAsync(id, role);
            if (!result) return BadRequest(new { message = "Invalid role or user not found." });
            return Ok(new { message = "User role updated successfully." });
        }

        // Get logged-in user's ID from JWT
        private Guid? GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }

        [HttpDelete("admin/{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminDeactivateUser(Guid id)
        {
            var result =
                await _userService.DeactivateUserAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "User deactivated successfully."
            });
        }

        // ============================================
        // ADMIN ACTIVATE USER
        // PUT: api/user/admin/{id}/activate
        // ============================================

        [HttpPut("admin/{id:guid}/activate")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminActivateUser(Guid id)
        {
            var result =
                await _userService.ActivateUserAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "User activated successfully."
            });
        }
    }
}