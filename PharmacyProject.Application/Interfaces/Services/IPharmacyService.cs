using PharmacyProject.Application.DTOs.Pharmacy;

namespace PharmacyProject.Application.Interfaces.Services
{
    public interface IPharmacyService
    {
        Task<PharmacyResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<PharmacyResponseDto>> GetAllAsync();


        Task<CreatePharmacyDto> CreateAsync(CreatePharmacyDto createPharmacyDto);
        Task UpdateAsync(UpdatePharmacyDto updatePharmacyDto);
        Task DeleteAsync(int id);
    }
}
