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

        public EmergencyResponsesController(
            IEmergencyResponseService service)
        {
            _service = service;
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

            return Ok(response);
        }

        // ============================================
        // GET ALL RESPONSES OF AN EMERGENCY
        // ============================================

        [HttpGet("emergency/{emergencyId:guid}")]
        public async Task<IActionResult> GetByEmergency(
            Guid emergencyId)
        {
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
        // UPDATE RESPONSE
        // ============================================

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] EmergencyResponse response)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var donorId = GetCurrentUserId();

            if (donorId == null)
                return Unauthorized();

            var existingResponse =
                await _service.GetByIdAsync(id);

            if (existingResponse == null)
            {
                return NotFound(new
                {
                    message = "Emergency response not found."
                });
            }

            // Only owner or admin can update
            if (existingResponse.DonorId != donorId.Value &&
                !IsAdmin())
            {
                return Forbid();
            }

            response.Id = id;

            // Do not allow frontend to change ownership
            response.DonorId =
                existingResponse.DonorId;

            response.EmergencyRequestId =
                existingResponse.EmergencyRequestId;

            var result =
                await _service.UpdateAsync(response);

            if (!result)
            {
                return BadRequest(new
                {
                    message =
                        "Unable to update emergency response."
                });
            }

            return Ok(new
            {
                message =
                    "Emergency response updated successfully."
            });
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
}