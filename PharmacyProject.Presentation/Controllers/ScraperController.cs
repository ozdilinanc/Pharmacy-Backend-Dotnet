using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;

namespace PharmacyProject.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ScraperController : ControllerBase
    {
        private readonly IEnumerable<IInsuranceScraperService> _scraperServices;

        public ScraperController(IEnumerable<IInsuranceScraperService> scraperServices)
        {
            _scraperServices = scraperServices;
        }

        [HttpGet("test-allianz")]
        public async Task<IActionResult> TestAllianzScraper()
        {
            var allianzService = _scraperServices.FirstOrDefault(s => s.InsuranceName == InsuranceCompanyEnum.Allianz.ToString());

            if (allianzService == null)
                return NotFound("Allianz botu DI konteynerinde bulunamadı.");

            var scrapedData = await allianzService.ScrapeAsync();

            return Ok(new
            {
                TotalCount = scrapedData.Count,
                Data = scrapedData
            });
        }

        [HttpGet("test-turkiye-sigorta")]
        public async Task<IActionResult> TestTurkiyeSigortaScraper()
        {
            var turkiyeSigortaService = _scraperServices.FirstOrDefault(s => s.InsuranceName == InsuranceCompanyEnum.TurkiyeSigorta.ToString());

            if (turkiyeSigortaService == null)
                return NotFound("Türkiye Sigorta botu DI konteynerinde bulunamadı.");

            var scrapedData = await turkiyeSigortaService.ScrapeAsync();

            return Ok(new
            {
                TotalCount = scrapedData.Count,
                Data = scrapedData
            });
        }

        [HttpGet("test-mapfree-sigorta")]
        public async Task<IActionResult> TestMapFre()
        {
            var mapFreService = _scraperServices.FirstOrDefault(s => s.InsuranceName == InsuranceCompanyEnum.MapfreSigorta.ToString());

            if (mapFreService == null)
                return NotFound("Mapfre botu DI konteynerinde bulunamadı.");

            var scrapedData = await mapFreService.ScrapeAsync();

            return Ok(new
            {
                TotalCount = scrapedData.Count,
                Data = scrapedData
            });
        }

        [HttpGet("test-eureko")]
        public async Task<IActionResult> TestEureko()
        {
            var eurekoService = _scraperServices.FirstOrDefault(s => s.InsuranceName == InsuranceCompanyEnum.EurekoSigorta.ToString());

            if (eurekoService == null)
                return NotFound("Eureko botu DI konteynerinde bulunamadı.");

            var scrapedData = await eurekoService.ScrapeAsync();

            return Ok(new
            {
                TotalCount = scrapedData.Count,
                Data = scrapedData
            });
        }

        [HttpGet("test-bupaAcibadem")]
        public async Task<IActionResult> TestBupaAcibadem()
        {
            var bupaAcibademService = _scraperServices.FirstOrDefault(s => s.InsuranceName == InsuranceCompanyEnum.BupaAcibademSigorta.ToString());

            if (bupaAcibademService == null)
                return NotFound("Bupa Acibadem botu DI konteynerinde bulunamadı.");

            var scrapedData = await bupaAcibademService.ScrapeAsync();

            return Ok(new
            {
                TotalCount = scrapedData.Count,
                Data = scrapedData
            });
        }

        [HttpGet("test-Axa")]
        public async Task<IActionResult> TestAxa()
        {
            var axaService = _scraperServices.FirstOrDefault(s => s.InsuranceName == InsuranceCompanyEnum.AxaSigorta.ToString());

            if (axaService == null)
                return NotFound("Axa botu DI konteynerinde bulunamadı.");

            var scrapedData = await axaService.ScrapeAsync();

            return Ok(new
            {
                TotalCount = scrapedData.Count,
                Data = scrapedData
            });
        }

        [HttpGet("test-anadolu")]
        public async Task<IActionResult> TestAnadolu()
        {
            var anadoluService = _scraperServices.FirstOrDefault(s => s.InsuranceName == InsuranceCompanyEnum.AnadoluSigorta.ToString());

            if (anadoluService == null)
                return NotFound("Anadolu Sigorta botu DI konteynerinde bulunamadı.");

            var scrapedData = await anadoluService.ScrapeAsync();

            return Ok(new
            {
                TotalCount = scrapedData.Count,
                Data = scrapedData
            });
        }

        [HttpGet("test-akSigorta")]
        public async Task<IActionResult> TestAkSigorta()
        {
            var akSigortaService = _scraperServices.FirstOrDefault(s => s.InsuranceName == InsuranceCompanyEnum.Aksigorta.ToString());

            if (akSigortaService == null)
                return NotFound("Ak Sigorta botu DI konteynerinde bulunamadı.");

            var scrapedData = await akSigortaService.ScrapeAsync();

            return Ok(new
            {
                TotalCount = scrapedData.Count,
                Data = scrapedData
            });
        }

    }
}