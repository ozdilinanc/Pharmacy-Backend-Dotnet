using PharmacyProject.Application.DTOs.Pharmacy;

using PharmacyProject.Application.DTOs.Common;

namespace PharmacyProject.Application.Interfaces.Services
{
    public interface IPharmacyService
    {
        Task<PharmacyResponseDto> GetByIdAsync(int id);
        Task<PagedResponse<PharmacyResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 50);
        Task<PagedResponse<PharmacyResponseDto>> GetByLocationAsync(string citySlug, string? districtSlug = null, bool? isOnDuty = null, List<int>? insuranceIds = null, int pageNumber = 1, int pageSize = 50);
        Task<PharmacyResponseDto> CreateAsync(CreatePharmacyDto createPharmacyDto);
        Task UpdateAsync(UpdatePharmacyDto updatePharmacyDto);
        Task DeleteAsync(int id);
    }
}
