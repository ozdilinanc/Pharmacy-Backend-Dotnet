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

        public async Task<CreatePharmacyDto> CreateAsync(CreatePharmacyDto createPharmacyDto)
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

            return createPharmacyDto;
        }

        public async Task<PharmacyResponseDto> GetByIdAsync(int id)
        {
            var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(id);

            if (pharmacy == null)
                throw new KeyNotFoundException("Bu ID'ye sahip eczane bulunamadı!");

            return new PharmacyResponseDto
            {
                Id = pharmacy.Id,
                Name = pharmacy.Name,
                Address = pharmacy.Address,
                PhoneNumber = pharmacy.PhoneNumber,
                Latitude = pharmacy.Latitude,
                Longitude = pharmacy.Longitude
            };
        }

        public async Task<IEnumerable<PharmacyResponseDto>> GetAllAsync()
        {
            var pharmacies = await _unitOfWork.Pharmacies.GetAllAsync();

            return pharmacies.Select(p => new PharmacyResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                PhoneNumber = p.PhoneNumber,
                Latitude = p.Latitude,
                Longitude = p.Longitude
            });
        }

        public async Task UpdateAsync(UpdatePharmacyDto updatePharmacyDto)
        {
            var existingPharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(updatePharmacyDto.Id);

            if (existingPharmacy == null)
                throw new KeyNotFoundException("Güncellenecek eczane bulunamadı!");

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
                throw new KeyNotFoundException("Silinecek eczane bulunamadı!");

            _unitOfWork.Pharmacies.Delete(pharmacyToDelete);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
