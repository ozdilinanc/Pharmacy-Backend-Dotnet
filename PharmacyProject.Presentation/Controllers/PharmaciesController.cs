using Microsoft.AspNetCore.Mvc;
using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.Interfaces.Services;

namespace PharmacyProject.Presentation.Controllers
{
    /// <summary>
    /// Eczane kayıtlarının eklendiği, listelendiği, güncellendiği ve silindiği yönetim uç noktasıdır.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PharmaciesController : ControllerBase
    {
        private readonly IPharmacyService _pharmacyService;

        public PharmaciesController(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _pharmacyService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _pharmacyService.GetByIdAsync(id));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // TODO: [Authorize] EKLENECEK
        public async Task<IActionResult> Create([FromBody] CreatePharmacyDto createDto)
        {
            var createdPharmacy = await _pharmacyService.CreateAsync(createDto);

            return CreatedAtAction(nameof(GetById), new { id = createdPharmacy.Id }, createdPharmacy);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdatePharmacyDto updateDto)
        {
            await _pharmacyService.UpdateAsync(updateDto);
            return Ok(new { message = "Eczane başarıyla güncellendi" });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _pharmacyService.DeleteAsync(id);
            return Ok(new { message = "Eczane başarıyla silindi" });
        }
    }
}