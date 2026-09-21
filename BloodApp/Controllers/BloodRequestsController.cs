using BloodDonationAPI.BLL.DTOs.BloodRequest;
using BloodDonationAPI.BLL.Interfaces;
using BloodDonationAPI.DAL.Repositories;
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
        private readonly IBloodRequestResponseRepository _bloodRequestResponseRepository;

        public BloodRequestsController(
            IBloodRequestService service,
            IBloodRequestResponseRepository bloodRequestResponseRepository)
        {
            _service = service;
            _bloodRequestResponseRepository = bloodRequestResponseRepository;
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
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var requests = await _service.GetPendingAsync();

            // If this logged-in donor previously responded to a request
            // and the requester rejected that response, hide ONLY that
            // request from this donor. The request remains Pending and
            // continues to appear normally for all other eligible users.
            var visibleRequests = new List<BloodRequestDto>();

            foreach (var request in requests)
            {
                var myResponse =
                    await _bloodRequestResponseRepository
                        .GetByRequestAndDonorAsync(
                            request.Id,
                            userId.Value);

                if (myResponse != null &&
                    myResponse.Status.Equals(
                        "Rejected",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                visibleRequests.Add(request);
            }

            return Ok(visibleRequests);
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
        // I CAN DONATE
        // ==========================================

        [HttpPost("{id:guid}/respond")]
        public async Task<IActionResult> Respond(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var response =
                    await _service.RespondAsync(
                        id,
                        userId.Value);

                if (response == null)
                {
                    return NotFound(new
                    {
                        message = "Blood request not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Your donation response has been sent successfully.",

                    response
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // ==========================================
        // GET DONOR RESPONSES
        // ==========================================

        [HttpGet("{id:guid}/responses")]
        public async Task<IActionResult> GetResponses(Guid id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            try
            {
                var responses =
                    await _service.GetResponsesAsync(
                        id,
                        userId.Value,
                        role);

                return Ok(responses);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = ex.Message
                    });
            }
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
        // CONFIRM DONOR RESPONSE
        // ==========================================

        [HttpPatch("{id:guid}/responses/{responseId:guid}/confirm")]
        public async Task<IActionResult> ConfirmResponse(
            Guid id,
            Guid responseId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            try
            {
                var response =
                    await _service.ConfirmResponseAsync(
                        id,
                        responseId,
                        userId.Value,
                        role);

                return Ok(new
                {
                    message = "Donor confirmed successfully.",
                    response
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = ex.Message
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // ==========================================
        // REJECT DONOR RESPONSE
        // ==========================================

        [HttpPatch("{id:guid}/responses/{responseId:guid}/reject")]
        public async Task<IActionResult> RejectResponse(
            Guid id,
            Guid responseId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            try
            {
                var response =
                    await _service.RejectResponseAsync(
                        id,
                        responseId,
                        userId.Value,
                        role);

                return Ok(new
                {
                    message = "Donor response rejected successfully.",
                    response
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = ex.Message
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // ==========================================
        // COMPLETE DONATION
        // ==========================================

        [HttpPatch("{id:guid}/responses/{responseId:guid}/complete")]
        public async Task<IActionResult> CompleteDonation(
            Guid id,
            Guid responseId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var role = GetCurrentUserRole();

            try
            {
                var response =
                    await _service.CompleteDonationAsync(
                        id,
                        responseId,
                        userId.Value,
                        role);

                return Ok(new
                {
                    message = "Donation completed successfully.",
                    response
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = ex.Message
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
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