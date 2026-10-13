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

        public BloodBankSettingController(IBloodBankSettingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bloodBanks = await _service.GetAllAsync();
            return Ok(bloodBanks);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bloodBank = await _service.GetByIdAsync(id);
            if (bloodBank == null)
                return NotFound(new { message = "Blood bank not found." });
            return Ok(bloodBank);
        }

        [HttpPost]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Add([FromBody] UpdateBloodBankSettingDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var bloodBank = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = bloodBank.Id }, bloodBank);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBloodBankSettingDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var bloodBank = await _service.UpdateAsync(id, dto);
            if (bloodBank == null)
                return NotFound(new { message = "Blood bank not found." });
            return Ok(bloodBank);
        }

        // Single contact-settings save endpoint for the mobile app.
        // Reuses the existing record when present; creates one otherwise.
        [HttpPut("settings")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> SaveSettings([FromBody] UpdateBloodBankSettingDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var bloodBanks = await _service.GetAllAsync();
            var existing = bloodBanks.OrderBy(b => b.Id).FirstOrDefault();

            if (existing != null)
            {
                var updated = await _service.UpdateAsync(existing.Id, dto);
                if (updated != null)
                    return Ok(updated);

                // The record may have been deleted between the read and update.
                return Conflict(new { message = "Blood bank settings changed. Please retry." });
            }

            var created = await _service.AddAsync(dto);
            return Ok(created);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Blood bank not found." });
            return Ok(new { message = "Blood bank deleted successfully." });
        }
    }
}
