using BloodDonationAPI.BLL.DTOs.BloodBank;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/blood-banks")]
    [Authorize]
    public class BloodBankSettingController : ControllerBase
    {
        private readonly IBloodBankSettingService _service;

        public BloodBankSettingController(
            IBloodBankSettingService service)
        {
            _service = service;
        }

        // ==========================================
        // GET ALL BLOOD BANKS
        // GET: api/blood-banks
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bloodBanks = await _service.GetAllAsync();

            return Ok(bloodBanks);
        }

        // ==========================================
        // GET BLOOD BANK BY ID
        // GET: api/blood-banks/{id}
        // ==========================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bloodBank = await _service.GetByIdAsync(id);

            if (bloodBank == null)
            {
                return NotFound(new
                {
                    message = "Blood bank not found."
                });
            }

            return Ok(bloodBank);
        }

        // ==========================================
        // ADD BLOOD BANK
        // POST: api/blood-banks
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Add(
            [FromBody] UpdateBloodBankSettingDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var bloodBank = await _service.AddAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = bloodBank.Id },
                bloodBank);
        }

        // ==========================================
        // UPDATE BLOOD BANK
        // PUT: api/blood-banks/{id}
        // ==========================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateBloodBankSettingDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var bloodBank =
                await _service.UpdateAsync(id, dto);

            if (bloodBank == null)
            {
                return NotFound(new
                {
                    message = "Blood bank not found."
                });
            }

            return Ok(bloodBank);
        }

        // ==========================================
        // DELETE BLOOD BANK
        // DELETE: api/blood-banks/{id}
        // ==========================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
            {
                return NotFound(new
                {
                    message = "Blood bank not found."
                });
            }

            return Ok(new
            {
                message = "Blood bank deleted successfully."
            });
        }
    }
}