using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.Interfaces.Services;

namespace PharmacyProject.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminManualMatchService _adminManualMatchService;

        public AdminController(IAdminManualMatchService adminManualMatchService)
        {
            _adminManualMatchService = adminManualMatchService;
        }

        [HttpGet("unmatched")]
        public async Task<IActionResult> GetUnmatchedPharmacies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _adminManualMatchService.GetUnmatchedPharmaciesAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPost("match")]
        public async Task<IActionResult> MatchPharmacy([FromBody] ManualMatchRequestDto matchRequestDto)
        {
            try
            {
                await _adminManualMatchService.MatchPharmacyAsync(matchRequestDto);
                return Ok(new { Message = "Eczane başarıyla eşleştirildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("unmatched/{id}")]
        public async Task<IActionResult> DeleteUnmatchedPharmacy(int id)
        {
            await _adminManualMatchService.DeleteUnmatchedPharmacyAsync(id);
            return Ok(new { Message = "Karantina kaydı silindi." });
        }

        [HttpPost("unmatched/{id}/approve")]
        public async Task<IActionResult> ApproveAsNewPharmacy(int id, [FromBody] ApproveAsNewDto dto)
        {
            try
            {
                await _adminManualMatchService.ApproveAsNewPharmacyAsync(id, dto);
                return Ok(new { Message = "Eczane başarıyla yeni kayıt olarak eklendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("unmatched/{id}/suggestions")]
        public async Task<IActionResult> GetSuggestions(int id)
        {
            try
            {
                var suggestions = await _adminManualMatchService.GetSuggestionsAsync(id);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
