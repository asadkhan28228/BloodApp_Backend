using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using BloodDonationAPI.BLL.DTOs.Emergency;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/emergencies")]
    [Authorize]
    public class EmergencyController : ControllerBase
    {
        private readonly IEmergencyService _emergencyService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<EmergencyController> _logger;

        public EmergencyController(
            IEmergencyService emergencyService,
            IHubContext<NotificationHub> hubContext,
            ILogger<EmergencyController> logger)
        {
            _emergencyService = emergencyService;
            _hubContext = hubContext;
            _logger = logger;
        }

        // =========================================================
        // GET: api/emergencies
        // Admin / BloodBankAdmin only
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = GetCurrentUserRole();

            if (!IsAdmin(role))
                return Forbid();

            var emergencies =
                await _emergencyService.GetAllAsync();

            return Ok(emergencies);
        }

        // =========================================================
        // GET: api/emergencies/active
        // All authenticated users
        // =========================================================

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var role = GetCurrentUserRole();
            var emergencies = await _emergencyService.GetActiveAsync();
            if (IsAdmin(role)) return Ok(emergencies);

            // Reporter and the first journey donor keep seeing the request.
            // All other donors stop receiving it from the active API.
            var visible = new List<EmergencyRequestDto>();
            foreach (var emergency in emergencies)
            {
                if (emergency.ReporterUid == userId.Value)
                {
                    visible.Add(emergency);
                    continue;
                }

                var responses = await _emergencyService.GetResponsesAsync(emergency.Id);
                var assigned = responses.FirstOrDefault(r =>
                    r.Status.Equals("On The Way", StringComparison.OrdinalIgnoreCase) ||
                    r.Status.Equals("Arrived", StringComparison.OrdinalIgnoreCase) ||
                    r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

                if (assigned == null || assigned.DonorId == userId.Value)
                    visible.Add(emergency);
            }

            return Ok(visible);
        }

        // =========================================================
        // GET: api/emergencies/my
        // Current user's own emergency requests
        // =========================================================

        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var emergencies =
                await _emergencyService
                    .GetMyRequestsAsync(userId.Value);

            return Ok(emergencies);
        }

        // =========================================================
        // GET: api/emergencies/{id}
        // Emergency details
        // Reporter or Admin can view
        // =========================================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            var emergency =
                await _emergencyService.GetByIdAsync(
                    id,
                    userId.Value,
                    role);

            if (emergency == null)
            {
                return NotFound(new
                {
                    message = "Emergency request not found."
                });
            }

            // Only reporter or admin can access private details.
            if (!IsAdmin(role) &&
                emergency.ReporterUid != userId.Value)
            {
                return Forbid();
            }

            return Ok(emergency);
        }

        // =========================================================
        // POST: api/emergencies
        // Create emergency
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmergencyRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var emergency =
                await _emergencyService.CreateAsync(
                    dto,
                    userId.Value);

            if (emergency == null)
            {
                return BadRequest(new
                {
                    message =
                        "Emergency could not be created."
                });
            }

            await BroadcastEmergencyUpdatedAsync(emergency.Id);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = emergency.Id
                },
                emergency);
        }

        // =========================================================
        // POST: api/emergencies/{id}/respond
        // Donor presses "I Can Help"
        // =========================================================

        [HttpPost("{id:guid}/respond")]
        public async Task<IActionResult> Respond(Guid id)
        {
            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            // Prevent new donor responses after a journey is assigned.
            var existingResponses = await _emergencyService.GetResponsesAsync(id);
            if (existingResponses.Any(r =>
                r.Status.Equals("On The Way", StringComparison.OrdinalIgnoreCase) ||
                r.Status.Equals("Arrived", StringComparison.OrdinalIgnoreCase) ||
                r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { message = "This emergency already has an assigned donor." });
            }

            var response =
                await _emergencyService.RespondAsync(
                    id,
                    donorId.Value);

            if (response == null)
            {
                return BadRequest(new
                {
                    message =
                        "You cannot respond to this emergency. " +
                        "It may be expired, inactive, already responded to, " +
                        "or your blood type may not be compatible."
                });
            }

            await BroadcastEmergencyUpdatedAsync(id);
            return Ok(response);
        }

        // =========================================================
        // GET: api/emergencies/{id}/responses
        // Reporter / Admin can see responding donors
        // =========================================================

        [HttpGet("{id:guid}/responses")]
        public async Task<IActionResult> GetResponses(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            var emergency =
                await _emergencyService.GetByIdAsync(
                    id,
                    userId.Value,
                    role);

            if (emergency == null)
            {
                return NotFound(new
                {
                    message =
                        "Emergency request not found."
                });
            }

            if (!IsAdmin(role) &&
                emergency.ReporterUid != userId.Value)
            {
                return Forbid();
            }

            var responses =
                await _emergencyService
                    .GetResponsesAsync(id);

            return Ok(responses);
        }

        // =========================================================
        // GET: api/emergencies/my-responses
        // Current donor's own responses
        // =========================================================

        [HttpGet("my-responses")]
        public async Task<IActionResult> GetMyResponses()
        {
            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            var responses =
                await _emergencyService
                    .GetMyResponsesAsync(donorId.Value);

            return Ok(responses);
        }

        // =========================================================
        // PATCH:
        // api/emergencies/responses/{responseId}/status
        //
        // Accepted
        // On The Way
        // Arrived
        // Completed
        // =========================================================

        [HttpPatch("responses/{responseId:guid}/status")]
        public async Task<IActionResult> UpdateResponseStatus(
            Guid responseId,
            [FromBody] UpdateEmergencyResponseStatusRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new
                {
                    message = "Response status is required."
                });
            }

            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            var success =
                await _emergencyService
                    .UpdateResponseStatusAsync(
                        responseId,
                        donorId.Value,
                        request.Status);

            if (!success)
            {
                return BadRequest(new
                {
                    message =
                        "Response status could not be updated."
                });
            }

            var updatedResponses = await _emergencyService.GetMyResponsesAsync(donorId.Value);
            var updatedResponse = updatedResponses.FirstOrDefault(r => r.Id == responseId);
            if (updatedResponse != null)
                await BroadcastEmergencyUpdatedAsync(updatedResponse.EmergencyRequestId);

            return Ok(new
            {
                message =
                    "Emergency response status updated successfully."
            });
        }

        // =========================================================
        // PATCH:
        // api/emergencies/responses/{responseId}/complete
        // =========================================================

        [HttpPatch("responses/{responseId:guid}/complete")]
        public async Task<IActionResult> CompleteResponse(
            Guid responseId)
        {
            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            var success =
                await _emergencyService
                    .CompleteResponseAsync(
                        responseId,
                        donorId.Value);

            if (!success)
            {
                return BadRequest(new
                {
                    message =
                        "Emergency response could not be completed."
                });
            }

            var completedResponses = await _emergencyService.GetMyResponsesAsync(donorId.Value);
            var completedResponse = completedResponses.FirstOrDefault(r => r.Id == responseId);
            if (completedResponse != null)
                await BroadcastEmergencyUpdatedAsync(completedResponse.EmergencyRequestId);

            return Ok(new
            {
                message =
                    "Emergency response completed successfully."
            });
        }

        // =========================================================
        // PATCH: api/emergencies/{id}/cancel
        // Reporter OR Admin
        // =========================================================

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            var success =
                await _emergencyService.CancelAsync(
                    id,
                    userId.Value,
                    role);

            if (!success)
            {
                return BadRequest(new
                {
                    message =
                        "Emergency could not be cancelled."
                });
            }

            await BroadcastEmergencyUpdatedAsync(id);

            return Ok(new
            {
                message =
                    "Emergency cancelled successfully."
            });
        }

        // =========================================================
        // PATCH: api/emergencies/{id}/resolve
        // Reporter OR Admin
        // =========================================================

        [HttpPatch("{id:guid}/resolve")]
        public async Task<IActionResult> Resolve(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            var success =
                await _emergencyService.ResolveAsync(
                    id,
                    userId.Value,
                    role);

            if (!success)
            {
                return BadRequest(new
                {
                    message =
                        "Emergency could not be resolved."
                });
            }

            await BroadcastEmergencyUpdatedAsync(id);

            return Ok(new
            {
                message =
                    "Emergency resolved successfully."
            });
        }

        private async Task BroadcastEmergencyUpdatedAsync(Guid emergencyId)
        {
            try
            {
                // Broadcast only the ID; clients fetch their own authorized view.
                // This also works after an emergency is deleted/cancelled.
                await _hubContext.Clients.All.SendAsync(
                    "EmergencyUpdated", new { emergencyId = emergencyId.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Could not publish live update for emergency {EmergencyId}", emergencyId);
            }
        }

        // =========================================================
        // CURRENT USER ID
        // =========================================================

        private Guid? GetCurrentUserId()
        {
            var claim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                claim =
                    User.FindFirst("sub");
            }

            if (claim == null)
                return null;

            return Guid.TryParse(
                claim.Value,
                out var userId)
                ? userId
                : null;
        }

        // =========================================================
        // CURRENT USER ROLE
        // =========================================================

        private string GetCurrentUserRole()
        {
            var role =
                User.FindFirst(
                    ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(role))
            {
                role =
                    User.FindFirst("role")?.Value;
            }

            return role ?? "user";
        }

        // =========================================================
        // ADMIN CHECK
        // =========================================================

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
    }

    // =============================================================
    // REQUEST DTO FOR RESPONSE STATUS
    // =============================================================

    public class UpdateEmergencyResponseStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}