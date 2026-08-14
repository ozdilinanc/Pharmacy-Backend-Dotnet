using PharmacyProject.Application.DTOs.External.Insurance;

namespace PharmacyProject.Application.Interfaces.Services
{
    public interface IPharmacyMatchingService
    {
        Task MatchAndProcessAsync(List<ScrapedPharmacyDto> scrapedPharmacies);
    }
}
