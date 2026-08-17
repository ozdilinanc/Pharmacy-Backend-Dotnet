using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.DTOs.Common;
using PharmacyProject.Core.Entities;

namespace PharmacyProject.Application.Interfaces.Services
{
    public interface IAdminManualMatchService
    {
        Task<PagedResponse<UnmatchedPharmacyDto>> GetUnmatchedPharmaciesAsync(int pageNumber = 1, int pageSize = 50);
        Task MatchPharmacyAsync(ManualMatchRequestDto matchRequestDto);
        Task DeleteUnmatchedPharmacyAsync(int unmatchedPharmacyId);
        Task<IEnumerable<PharmacyResponseDto>> GetSuggestionsAsync(int unmatchedPharmacyId);
    }
}
