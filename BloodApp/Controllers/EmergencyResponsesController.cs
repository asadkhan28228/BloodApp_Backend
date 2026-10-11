using BloodDonationAPI.API.Hubs;
using BloodDonationAPI.DAL.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/emergency-responses")]
    [Authorize]
    public class EmergencyResponsesController : ControllerBase
    {
        private readonly IEmergencyResponseService _service;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly AppDbContext _db;

        public EmergencyResponsesController(
            IEmergencyResponseService service,
            IHubContext<NotificationHub> hub,
            AppDbContext db)
        {
            _service = service;
            _hub = hub;
            _db = db;
        }

        // ============================================
        // GET RESPONSE BY ID
        // ============================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response =
                await _service.GetByIdAsync(id);

            if (response == null)
            {
                return NotFound(new
                {
                    message = "Emergency response not found."
                });
            }

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            if (response.DonorId != userId.Value && !IsAdmin())
            {
                var reporterId = await _db.EmergencyRequests.AsNoTracking()
                    .Where(e => e.Id == response.EmergencyRequestId)
                    .Select(e => (Guid?)e.ReporterUid)
                    .FirstOrDefaultAsync();
                if (reporterId != userId.Value) return Forbid();
            }
            return Ok(response);
        }

        // ============================================
        // GET ALL RESPONSES OF AN EMERGENCY
        // ============================================

        [HttpGet("emergency/{emergencyId:guid}")]
        public async Task<IActionResult> GetByEmergency(
            Guid emergencyId)
        {
            if (!IsAdmin()) return Forbid();
            var responses =
                await _service.GetByEmergencyAsync(
                    emergencyId);

            return Ok(responses);
        }

        // ============================================
        // GET CURRENT DONOR'S RESPONSES
        // ============================================

        [HttpGet("my")]
        public async Task<IActionResult> GetMyResponses()
        {
            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            var responses =
                await _service.GetByDonorAsync(
                    donorId.Value);

            return Ok(responses);
        }

        // ============================================
        // CHECK MY RESPONSE FOR AN EMERGENCY
        // ============================================

        [HttpGet("emergency/{emergencyId:guid}/my")]
        public async Task<IActionResult> GetMyEmergencyResponse(
            Guid emergencyId)
        {
            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            var response =
                await _service.GetByEmergencyAndDonorAsync(
                    emergencyId,
                    donorId.Value);

            if (response == null)
            {
                return NotFound(new
                {
                    message = "You have not responded to this emergency."
                });
            }

            return Ok(response);
        }

        // ============================================
        // CREATE DONOR RESPONSE
        // ============================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] EmergencyResponse response)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            // Never trust DonorId from frontend
            response.DonorId = donorId.Value;

            var result =
                await _service.CreateAsync(response);

            if (result == null)
            {
                return BadRequest(new
                {
                    message =
                        "Unable to respond to this emergency. " +
                        "The emergency may be inactive, " +
                        "expired, or you may not be eligible."
                });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        // ============================================
        // DELETE MY RESPONSE
        // ============================================

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid id)
        {
            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            var response =
                await _service.GetByIdAsync(id);

            if (response == null)
            {
                return NotFound(new
                {
                    message = "Emergency response not found."
                });
            }

            // Donor can delete only his own response
            if (response.DonorId != donorId.Value &&
                !IsAdmin())
            {
                return Forbid();
            }

            var result =
                await _service.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Emergency response not found."
                });
            }

            return Ok(new
            {
                message =
                    "Emergency response deleted successfully."
            });
        }

        // ============================================
        // DONOR LIVE LOCATION UPDATE
        // PUT: api/emergency-responses/{id}/location
        // ============================================
        [HttpPut("{id:guid}/location")]
        public async Task<IActionResult> UpdateLocation(
            Guid id, [FromBody] UpdateEmergencyLocationRequest request)
        {
            var donorId = GetCurrentUserId();
            if (donorId == null) return Unauthorized();
            if (request == null ||
                request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180 ||
                double.IsNaN(request.Latitude) || double.IsNaN(request.Longitude) ||
                double.IsInfinity(request.Latitude) || double.IsInfinity(request.Longitude))
                return BadRequest(new { message = "Invalid coordinates." });

            var updated = await _service.UpdateLocationAsync(
                id, donorId.Value, request.Latitude, request.Longitude);
            if (!updated)
                return BadRequest(new { message = "Response not found, not owned by you, or closed." });
            // Only the reporter and this donor receive sensitive location updates.
            var response = await _service.GetByIdAsync(id);
            if (response != null)
            {
                var emergency = await _db.EmergencyRequests.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == response.EmergencyRequestId);
                var locationEvent = new
                {
                    responseId = id,
                    emergencyRequestId = response.EmergencyRequestId,
                    donorId = donorId.Value,
                    latitude = request.Latitude,
                    longitude = request.Longitude,
                    updatedAt = DateTime.UtcNow
                };
                await _hub.Clients.Group($"user_{donorId.Value}")
                    .SendAsync("DonorLocationUpdated", locationEvent);
                if (emergency != null && emergency.ReporterUid != donorId.Value)
                    await _hub.Clients.Group($"user_{emergency.ReporterUid}")
                        .SendAsync("DonorLocationUpdated", locationEvent);

                // Admin dashboards use authenticated private user groups.
                var adminIds = await _db.Users.AsNoTracking()
                    .Where(u => u.IsActive && (u.Role == "admin" || u.Role == "bloodBankAdmin"))
                    .Select(u => u.Id)
                    .ToListAsync();
                foreach (var adminId in adminIds)
                    await _hub.Clients.Group($"user_{adminId}")
                        .SendAsync("DonorLocationUpdated", locationEvent);
            }
            return Ok(new { message = "Location updated successfully." });
        }

        // ============================================
        // CURRENT USER ID
        // ============================================

        private Guid? GetCurrentUserId()
        {
            var claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (Guid.TryParse(claim, out var userId))
                return userId;

            return null;
        }

        // ============================================
        // ADMIN CHECK
        // ============================================

        private bool IsAdmin()
        {
            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);

            return role?.Equals(
                       "admin",
                       StringComparison.OrdinalIgnoreCase) == true
                   ||
                   role?.Equals(
                       "bloodBankAdmin",
                       StringComparison.OrdinalIgnoreCase) == true;
        }
    }
    public class UpdateEmergencyLocationRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}