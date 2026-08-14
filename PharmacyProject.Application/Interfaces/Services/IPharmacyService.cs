using PharmacyProject.Application.DTOs.Pharmacy;

namespace PharmacyProject.Application.Interfaces.Services
{
    public interface IPharmacyService
    {
        Task<PharmacyResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<PharmacyResponseDto>> GetAllAsync();
        Task<IEnumerable<PharmacyResponseDto>> GetByLocationAsync(string citySlug, string? districtSlug = null, bool? isOnDuty = null);
        Task<PharmacyResponseDto> CreateAsync(CreatePharmacyDto createPharmacyDto);
        Task UpdateAsync(UpdatePharmacyDto updatePharmacyDto);
        Task DeleteAsync(int id);
    }
}
