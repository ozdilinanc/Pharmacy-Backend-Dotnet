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

        /// <summary>
        /// Sistemdeki tüm eczaneleri listeler.
        /// </summary>
        /// <returns>Eczanelerin listesini döndürür.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var pharmacies = await _pharmacyService.GetAllAsync();
            return Ok(pharmacies);
        }

        /// <summary>
        /// ID'si verilen tek bir eczanenin detaylarını getirir.
        /// </summary>
        /// <param name="id">Aranacak eczanenin benzersiz ID değeri</param>
        /// <returns>Bulunan eczanenin detayları</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Sisteme yeni bir eczane ekler.
        /// </summary>
        /// <param name="createDto">Eklenecek eczanenin bilgileri</param>
        /// <returns>Eklenen eczanenin bilgileri döner</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)] // TODO: İleride bunu 201 Created'a çevirebiliriz
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // TODO: [Authorize] EKLENECEK
        public async Task<IActionResult> Create([FromBody] CreatePharmacyDto createDto)
        {
            var createdPharmacy = await _pharmacyService.CreateAsync(createDto);
            return Ok(createdPharmacy);
        }

        /// <summary>
        /// Mevcut bir eczanenin bilgilerini günceller.
        /// </summary>
        /// <param name="updateDto">Güncellenecek eczanenin ID'si ve yeni bilgileri</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// ID'si verilen eczaneyi sistemden siler.
        /// </summary>
        /// <param name="id">Silinecek eczanenin benzersiz ID değeri</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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