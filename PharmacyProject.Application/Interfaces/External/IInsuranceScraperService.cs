using PharmacyProject.Application.DTOs.External.Insurance;

namespace PharmacyProject.Application.Interfaces.External
{
    public interface IInsuranceScraperService
    {
        string InsuranceName { get; }
        Task<List<ScrapedPharmacyDto>> ScrapeAsync();
    }
}