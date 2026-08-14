using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Core.Entities;

namespace PharmacyProject.Application.Interfaces.Services
{
    public interface IAdminManualMatchService
    {
        Task<IEnumerable<UnmatchedPharmacyDto>> GetUnmatchedPharmaciesAsync();
        Task MatchPharmacyAsync(ManualMatchRequestDto matchRequestDto);
        Task DeleteUnmatchedPharmacyAsync(int unmatchedPharmacyId);
        Task<IEnumerable<PharmacyResponseDto>> GetSuggestionsAsync(int unmatchedPharmacyId);
    }
}
