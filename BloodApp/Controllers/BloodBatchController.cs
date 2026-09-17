using BloodDonationAPI.BLL.DTOs.BloodBatch;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/blood-batches")]
    [Authorize]
    public class BloodBatchController : ControllerBase
    {
        private readonly IBloodBatchService
            _batchService;

        public BloodBatchController(
            IBloodBatchService batchService)
        {
            _batchService = batchService;
        }

        // ============================================
        // GET ALL BATCHES
        // ============================================

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll()
        {
            var batches =
                await _batchService.GetAllAsync();

            return Ok(batches);
        }

        // ============================================
        // GET BATCH BY ID
        // ============================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id)
        {
            var batch =
                await _batchService
                    .GetByIdAsync(id);

            if (batch == null)
            {
                return NotFound(new
                {
                    message =
                        "Blood batch not found."
                });
            }

            return Ok(batch);
        }

        // ============================================
        // GET BY BLOOD TYPE
        // ============================================

        [HttpGet("blood-type/{bloodType}")]
        public async Task<IActionResult> GetByBloodType(
            string bloodType)
        {
            var batches =
                await _batchService
                    .GetByBloodTypeAsync(
                        bloodType);

            return Ok(batches);
        }

        // ============================================
        // ADD BATCH
        // ============================================

        [HttpPost]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Add(
            [FromBody] CreateBloodBatchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var batch =
                    await _batchService
                        .AddAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        id = batch.Id
                    },
                    batch);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================
        // UPDATE BATCH
        // ============================================

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBloodBatchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result =
                    await _batchService
                        .UpdateAsync(id, dto);

                if (!result)
                {
                    return BadRequest(new
                    {
                        message =
                            "Unable to update blood batch."
                    });
                }

                var updated =
                    await _batchService
                        .GetByIdAsync(id);

                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================
        // DELETE BATCH
        // ============================================

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Delete(
            Guid id)
        {
            var result =
                await _batchService
                    .DeleteAsync(id);

            if (!result)
            {
                return BadRequest(new
                {
                    message =
                        "Unable to delete blood batch."
                });
            }

            return Ok(new
            {
                message =
                    "Blood batch deleted successfully."
            });
        }

        // ============================================
        // MARK EXPIRED
        // ============================================

        [HttpPost("mark-expired")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> MarkExpired()
        {
            var count =
                await _batchService
                    .MarkExpiredBatchesAsync();

            return Ok(new
            {
                message =
                    "Expired batches processed successfully.",

                expiredBatches = count
            });
        }

    }
}