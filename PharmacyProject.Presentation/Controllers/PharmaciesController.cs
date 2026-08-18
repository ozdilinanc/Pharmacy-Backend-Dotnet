using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.Interfaces.Services;

namespace PharmacyProject.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]      
    public class PharmaciesController : ControllerBase
    {
        private readonly IPharmacyService _pharmacyService;

        public PharmaciesController(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }

        [HttpGet("{citySlug}/{districtSlug?}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByLocation(string citySlug, string? districtSlug = null, [FromQuery] bool? isOnDuty = null, [FromQuery] List<int>? insuranceIds = null)
        {
            var pharmacies = await _pharmacyService.GetByLocationAsync(citySlug, districtSlug, isOnDuty, insuranceIds);
            return Ok(pharmacies);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _pharmacyService.GetAllAsync());
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var pharmacy = await _pharmacyService.GetByIdAsync(id);
            return Ok(pharmacy);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePharmacyDto createPharmacyDto)
        {
            var createdPharmacy = await _pharmacyService.CreateAsync(createPharmacyDto);
            return CreatedAtAction(nameof(GetById), new { id = createdPharmacy.Id }, createdPharmacy);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePharmacyDto updatePharmacyDto)
        {
            if (id != updatePharmacyDto.Id)
            {
                return BadRequest(new { message = "ID'ler uyuşmuyor!" });
            }

            await _pharmacyService.UpdateAsync(updatePharmacyDto);
            return NoContent();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _pharmacyService.DeleteAsync(id);
            return NoContent();
        }
    }
}
