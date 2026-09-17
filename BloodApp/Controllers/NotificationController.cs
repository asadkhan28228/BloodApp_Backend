using System.Security.Claims;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // ==========================================
        // GET MY PRIVATE NOTIFICATIONS
        // GET: api/notifications
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var notifications =
                await _notificationService.GetMyNotificationsAsync(userId.Value);

            return Ok(notifications);
        }

        // ==========================================
        // GET UNREAD COUNT
        // GET: api/notifications/unread-count
        // ==========================================

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var count =
                await _notificationService.GetUnreadCountAsync(userId.Value);

            return Ok(new
            {
                unreadCount = count
            });
        }

        // ==========================================
        // GET COMMUNITY NOTIFICATIONS
        // GET: api/notifications/community
        // ==========================================

        [HttpGet("community")]
        public async Task<IActionResult> GetCommunityNotifications()
        {
            var notifications =
                await _notificationService.GetCommunityNotificationsAsync();

            return Ok(notifications);
        }

        // ==========================================
        // MARK ONE AS READ
        // PATCH: api/notifications/{id}/read
        // ==========================================

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var success =
                await _notificationService.MarkAsReadAsync(
                    id,
                    userId.Value);

            if (!success)
            {
                return NotFound(new
                {
                    message = "Notification not found."
                });
            }

            return Ok(new
            {
                message = "Notification marked as read."
            });
        }

        // ==========================================
        // MARK ALL AS READ
        // PATCH: api/notifications/read-all
        // ==========================================

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId.Value);

            return Ok(new
            {
                message = "All notifications marked as read."
            });
        }

        // ==========================================
        // DELETE ONE NOTIFICATION
        // DELETE: api/notifications/{id}
        // ==========================================

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var success =
                await _notificationService.DeleteAsync(
                    id,
                    userId.Value);

            if (!success)
            {
                return NotFound(new
                {
                    message = "Notification not found."
                });
            }

            return Ok(new
            {
                message = "Notification deleted successfully."
            });
        }

        // ==========================================
        // GET CURRENT USER ID FROM JWT
        // ==========================================

        private Guid? GetCurrentUserId()
        {
            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                claim = User.FindFirst("sub");
            }

            if (claim == null)
                return null;

            return Guid.TryParse(
                claim.Value,
                out var userId)
                ? userId
                : null;
        }
    }
}