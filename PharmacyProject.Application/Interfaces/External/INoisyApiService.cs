using PharmacyProject.Application.DTOs.External.NosyApi;

namespace PharmacyProject.Application.Interfaces.External
{
    public interface INosyApiService
    {
        Task<List<NosyApiRecentPharmacyDto>> GetRecentPharmaciesAsync();
        Task<List<NosyApiOnDutyPharmacyDto>> GetOnDutyPharmaciesAsync(string city);
    }
}