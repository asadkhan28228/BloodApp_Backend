using BloodDonationAPI.BLL.DTOs.BloodInventory;
using BloodDonationAPI.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.API.Controllers
{
    [ApiController]
    [Route("api/blood-inventory")]
    [Authorize]
    public class BloodInventoryController : ControllerBase
    {
        private readonly IBloodInventoryService
            _inventoryService;

        public BloodInventoryController(
            IBloodInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // ============================================
        // GET ALL INVENTORY
        // ============================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var inventory =
                await _inventoryService.GetAllAsync();

            return Ok(inventory);
        }

        // ============================================
        // GET BY BLOOD TYPE
        // ============================================

        [HttpGet("{bloodType}")]
        public async Task<IActionResult> GetByBloodType(
            string bloodType)
        {
            var inventory =
                await _inventoryService
                    .GetByBloodTypeAsync(bloodType);

            if (inventory == null)
            {
                return NotFound(new
                {
                    message = "Blood inventory not found."
                });
            }

            return Ok(inventory);
        }

        // ============================================
        // UPDATE INVENTORY
        // ADMIN / BLOOD BANK ADMIN
        // ============================================

        [HttpPut]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Update(
            [FromBody] UpdateBloodInventoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result =
                await _inventoryService
                    .UpdateAsync(dto);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Unable to update blood inventory."
                });
            }

            var updated =
                await _inventoryService
                    .GetByBloodTypeAsync(dto.BloodType);

            return Ok(updated);
        }

        // ============================================
        // INCREASE STOCK
        // ADMIN / BLOOD BANK ADMIN
        // ============================================

        [HttpPatch("{bloodType}/increase")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Increase(
            string bloodType,
            [FromQuery] int units)
        {
            if (units <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Units must be greater than zero."
                });
            }

            var result =
                await _inventoryService
                    .IncreaseUnitsAsync(
                        bloodType,
                        units);

            if (!result)
            {
                return BadRequest(new
                {
                    message =
                        "Unable to increase blood inventory."
                });
            }

            var updated =
                await _inventoryService
                    .GetByBloodTypeAsync(bloodType);

            return Ok(updated);
        }

        // ============================================
        // DECREASE STOCK
        // ADMIN / BLOOD BANK ADMIN
        // ============================================

        [HttpPatch("{bloodType}/decrease")]
        [Authorize(Roles = "admin,bloodBankAdmin")]
        public async Task<IActionResult> Decrease(
            string bloodType,
            [FromQuery] int units)
        {
            if (units <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Units must be greater than zero."
                });
            }

            var result =
                await _inventoryService
                    .DecreaseUnitsAsync(
                        bloodType,
                        units);

            if (!result)
            {
                return BadRequest(new
                {
                    message =
                        "Insufficient blood inventory or blood type not found."
                });
            }

            var updated =
                await _inventoryService
                    .GetByBloodTypeAsync(bloodType);

            return Ok(updated);
        }
    }
}