using BloodDonationAPI.BLL.DTOs.Notification;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/community-notifications")]
    [Authorize]
    public class CommunityNotificationsController : ControllerBase
    {
        private readonly ICommunityNotificationService _service;

        public CommunityNotificationsController(
            ICommunityNotificationService service)
        {
            _service = service;
        }

        // ==========================================
        // GET ALL COMMUNITY NOTIFICATIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _service.GetAllAsync();

            return Ok(notifications);
        }

        // ==========================================
        // GET MY CREATED NOTIFICATIONS
        // ==========================================

        [HttpGet("my")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var notifications =
                await _service.GetByReporterAsync(userId.Value);

            return Ok(notifications);
        }

        // ==========================================
        // GET SINGLE NOTIFICATION
        // ==========================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var notification =
                await _service.GetByIdAsync(id);

            if (notification == null)
                return NotFound(new
                {
                    message = "Notification not found."
                });

            return Ok(notification);
        }

        // ==========================================
        // CREATE NOTIFICATION
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CommunityNotificationDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            // Reporter identity should come from JWT,
            // not directly from the mobile app.
            dto.ReporterUid = userId.Value;

            var notification =
                await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = notification.Id },
                notification);
        }

        // ==========================================
        // MARK AS READ
        // ==========================================

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result =
                await _service.MarkAsReadAsync(id);

            if (!result)
                return NotFound(new
                {
                    message = "Notification not found."
                });

            return Ok(new
            {
                message = "Notification marked as read."
            });
        }

        // ==========================================
        // DELETE
        // ==========================================

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result =
                await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new
                {
                    message = "Notification not found."
                });

            return Ok(new
            {
                message = "Notification deleted successfully."
            });
        }

        // ==========================================
        // CURRENT USER ID
        // ==========================================

        private Guid? GetCurrentUserId()
        {
            var claim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(claim, out var userId))
                return userId;

            return null;
        }
    }
}