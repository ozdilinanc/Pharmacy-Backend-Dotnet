using PharmacyProject.Application.DTOs.Common;
using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Application.Interfaces.Services;
using PharmacyProject.Core.Entities;

namespace PharmacyProject.Application.Services
{
    public class PharmacyService : IPharmacyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PharmacyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PharmacyResponseDto> CreateAsync(CreatePharmacyDto createPharmacyDto)
        {
            var newPharmacy = new Pharmacy
            {
                Name = createPharmacyDto.Name,
                Address = createPharmacyDto.Address,
                PhoneNumber = createPharmacyDto.PhoneNumber,
                Latitude = createPharmacyDto.Latitude,
                Longitude = createPharmacyDto.Longitude,
                DistrictId = createPharmacyDto.DistrictId
            };

            await _unitOfWork.Pharmacies.AddAsync(newPharmacy);
            await _unitOfWork.SaveChangesAsync();

            return new PharmacyResponseDto
            {
                Id = newPharmacy.Id,
                Name = newPharmacy.Name,
                Address = newPharmacy.Address,
                PhoneNumber = newPharmacy.PhoneNumber,
                Latitude = newPharmacy.Latitude,
                Longitude = newPharmacy.Longitude,
                DistrictId = newPharmacy.DistrictId
            };
        }

        public async Task<PharmacyResponseDto> GetByIdAsync(int id)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(id);

            if (pharmacy == null)
                throw new KeyNotFoundException("Bu ID'ye sahip eczane bulunamadi!");

            return new PharmacyResponseDto
            {
                Id = pharmacy.Id,
                Name = pharmacy.Name,
                Address = pharmacy.Address,
                PhoneNumber = pharmacy.PhoneNumber,
                Latitude = pharmacy.Latitude,
                Longitude = pharmacy.Longitude,
                DistrictId = pharmacy.DistrictId
            };
        }

        public async Task<PagedResponse<PharmacyResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 50)
        {
            var result = await _unitOfWork.Pharmacies.GetPharmaciesWithDetailsAsync(null, pageNumber, pageSize);

            var mappedData = result.Pharmacies.Select(p => new PharmacyResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                PhoneNumber = p.PhoneNumber,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                DistrictId = p.DistrictId,
                DistrictName = p.District?.Name
            });
            
            return new PagedResponse<PharmacyResponseDto>(mappedData, result.TotalCount, pageNumber, pageSize);
        }
        
        public async Task<PagedResponse<PharmacyResponseDto>> GetByLocationAsync(string citySlug, string? districtSlug = null, bool? isOnDuty = null, List<int>? insuranceIds = null, int pageNumber = 1, int pageSize = 50)
        {
            var result = await _unitOfWork.Pharmacies.GetPharmaciesWithDetailsAsync(p => 
                (string.IsNullOrEmpty(citySlug) || p.District.City.Slug == citySlug) &&
                (string.IsNullOrEmpty(districtSlug) || p.District.Slug == districtSlug) &&
                (!isOnDuty.HasValue || p.IsOnDuty == isOnDuty.Value) &&
                (insuranceIds == null || !insuranceIds.Any() || p.PharmacyInsurances.Any(pi => insuranceIds.Contains(pi.InsuranceCompanyId))),
                pageNumber, pageSize
            );

            var mappedData = result.Pharmacies.Select(p => new PharmacyResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                PhoneNumber = p.PhoneNumber,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                DistrictId = p.DistrictId,
                DistrictName = p.District?.Name,
                SupportedInsurances = p.PharmacyInsurances.Select(pi => new SupportedInsuranceDto 
                { 
                    Id = pi.InsuranceCompany.Id, 
                    Name = pi.InsuranceCompany.Name 
                }).ToList()
            });
            
            return new PagedResponse<PharmacyResponseDto>(mappedData, result.TotalCount, pageNumber, pageSize);
        }

        public async Task UpdateAsync(UpdatePharmacyDto updatePharmacyDto)
        {
            var existingPharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(updatePharmacyDto.Id);

            if (existingPharmacy == null)
                throw new KeyNotFoundException("Guncellenecek eczane bulunamadi!");

            existingPharmacy.Name = updatePharmacyDto.Name;
            existingPharmacy.Address = updatePharmacyDto.Address;
            existingPharmacy.PhoneNumber = updatePharmacyDto.PhoneNumber;
            existingPharmacy.Latitude = updatePharmacyDto.Latitude;
            existingPharmacy.Longitude = updatePharmacyDto.Longitude;
            existingPharmacy.DistrictId = updatePharmacyDto.DistrictId;

            _unitOfWork.Pharmacies.Update(existingPharmacy);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var pharmacyToDelete = await _unitOfWork.Pharmacies.GetByIdAsync(id);

            if (pharmacyToDelete == null)
                throw new KeyNotFoundException("Silinecek eczane bulunamadi!");

            _unitOfWork.Pharmacies.Delete(pharmacyToDelete);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
