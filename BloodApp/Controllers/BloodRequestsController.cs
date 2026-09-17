using BloodDonationAPI.BLL.DTOs.BloodRequest;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/blood-requests")]
    [Authorize]
    public class BloodRequestsController : ControllerBase
    {
        private readonly IBloodRequestService _service;

        public BloodRequestsController(
            IBloodRequestService service)
        {
            _service = service;
        }

        // ==========================================
        // GET ALL BLOOD REQUESTS
        // Admin / Blood Bank Admin
        // ==========================================

        [HttpGet]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _service.GetAllAsync();

            return Ok(requests);
        }

        // ==========================================
        // GET MY BLOOD REQUESTS
        // ==========================================

        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var requests =
                await _service.GetMyRequestsAsync(userId.Value);

            return Ok(requests);
        }

        // ==========================================
        // GET PENDING REQUESTS
        // ==========================================

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var requests = await _service.GetPendingAsync();

            return Ok(requests);
        }

        // ==========================================
        // GET SINGLE REQUEST
        // ==========================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            var request =
                await _service.GetByIdAsync(
                    id,
                    userId.Value,
                    role);

            if (request == null)
                return NotFound(new
                {
                    message = "Blood request not found."
                });

            return Ok(request);
        }

        // ==========================================
        // CREATE BLOOD REQUEST
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBloodRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var request =
                await _service.CreateAsync(
                    dto,
                    userId.Value);

            if (request == null)
            {
                return BadRequest(new
                {
                    message = "Unable to create blood request."
                });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = request.Id },
                request);
        }

        // ==========================================
        // UPDATE BLOOD REQUEST
        // ==========================================

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBloodRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            var result =
                await _service.UpdateAsync(
                    id,
                    dto,
                    userId.Value,
                    role);

            if (!result)
            {
                return NotFound(new
                {
                    message =
                        "Blood request not found or you are not allowed to update it."
                });
            }

            return Ok(new
            {
                message = "Blood request updated successfully."
            });
        }

        // ==========================================
        // CANCEL BLOOD REQUEST
        // ==========================================

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            var result =
                await _service.CancelAsync(
                    id,
                    userId.Value,
                    role);

            if (!result)
            {
                return NotFound(new
                {
                    message =
                        "Blood request not found or you are not allowed to cancel it."
                });
            }

            return Ok(new
            {
                message = "Blood request cancelled successfully."
            });
        }

        // ==========================================
        // COMPLETE BLOOD REQUEST
        // ==========================================

        [HttpPatch("{id:guid}/complete")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var role = GetCurrentUserRole();

            var result =
                await _service.CompleteAsync(
                    id,
                    role);

            if (!result)
            {
                return NotFound(new
                {
                    message =
                        "Blood request not found or cannot be completed."
                });
            }

            return Ok(new
            {
                message = "Blood request completed successfully."
            });
        }

        // ==========================================
        // CURRENT USER ID
        // ==========================================

        private Guid? GetCurrentUserId()
        {
            var claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (Guid.TryParse(claim, out var userId))
                return userId;

            return null;
        }

        // ==========================================
        // CURRENT USER ROLE
        // ==========================================

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(
                       ClaimTypes.Role)
                   ?? "user";
        }
    }
}