using Microsoft.AspNetCore.Mvc;
using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.Interfaces.Services;

namespace PharmacyProject.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmaciesController : ControllerBase
    {
        private readonly IPharmacyService _pharmacyService;

        public PharmaciesController(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }


        // GET: api/pharmacies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pharmacies = await _pharmacyService.GetAllAsync();
            return Ok(pharmacies);
        }

        // GET: api/pharmacies/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var pharmacy = await _pharmacyService.GetByIdAsync(id);
                return Ok(pharmacy);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/pharmacies
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePharmacyDto createDto)
        {
            var createdPharmacy = await _pharmacyService.CreateAsync(createDto);
            return Ok(createdPharmacy);
        }

        // PUT: api/pharmacies
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePharmacyDto updateDto)
        {
            try
            {
                await _pharmacyService.UpdateAsync(updateDto);
                return Ok(new { message = "Eczane başarıyla güncellendi" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //DELETE: api/pharmacies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _pharmacyService.DeleteAsync(id);
                return Ok(new { message = "Eczane başarıyla silindi" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}