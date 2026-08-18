using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using PharmacyProject.Core.Entities;

using PharmacyProject.Application.DTOs.Common;

namespace PharmacyProject.Application.Services
{
    public class AdminManualMatchService : IAdminManualMatchService
    {
        private readonly IUnmatchedPharmacyRepository _unmatchedPharmacyRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IPharmacyInsuranceRepository _pharmacyInsuranceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminManualMatchService> _logger;

        public AdminManualMatchService(
            IUnmatchedPharmacyRepository unmatchedPharmacyRepository,
            IPharmacyRepository pharmacyRepository,
            IPharmacyInsuranceRepository pharmacyInsuranceRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminManualMatchService> logger)
        {
            _unmatchedPharmacyRepository = unmatchedPharmacyRepository;
            _pharmacyRepository = pharmacyRepository;
            _pharmacyInsuranceRepository = pharmacyInsuranceRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResponse<UnmatchedPharmacyDto>> GetUnmatchedPharmaciesAsync(int pageNumber = 1, int pageSize = 50)
        {
            var result = await _unmatchedPharmacyRepository.GetUnmatchedWithDetailsAsync(u => !u.IsResolved, pageNumber, pageSize);
            
            var mappedData = result.UnmatchedPharmacies.Select(u => new UnmatchedPharmacyDto
            {
                Id = u.Id,
                ScrapedName = u.ScrapedName,
                ScrapedAddress = u.ScrapedAddress,
                ScrapedPhoneNumber = u.ScrapedPhoneNumber,
                SourceInsurance = u.SourceInsurance,
                DataSource = u.DataSource,
                CityId = u.CityId,
                DistrictId = u.DistrictId,
                ScrapedLatitude = u.ScrapedLatitude,
                ScrapedLongitude = u.ScrapedLongitude,
                CreatedAt = u.CreatedAt
            });

            return new PagedResponse<UnmatchedPharmacyDto>(mappedData, result.TotalCount, pageNumber, pageSize);
        }

        public async Task MatchPharmacyAsync(ManualMatchRequestDto matchRequestDto)
        {
            var unmatched = await _unmatchedPharmacyRepository.GetByIdAsync(matchRequestDto.UnmatchedPharmacyId);
            if (unmatched == null || unmatched.IsResolved)
            {
                throw new Exception("Karantina kaydi bulunamadi veya zaten cozumlenmis.");
            }

            var realPharmacy = await _pharmacyRepository.GetByIdAsync(matchRequestDto.RealPharmacyId);
            if (realPharmacy == null)
            {
                throw new Exception("Hedef eczane veritabaninda bulunamadi.");
            }

            if (unmatched.SourceInsurance.HasValue)
            {
                var existingRelation = await _pharmacyInsuranceRepository.FindAsync(pi => 
                    pi.PharmacyId == realPharmacy.Id && 
                    pi.InsuranceCompanyId == (int)unmatched.SourceInsurance.Value);

                if (!existingRelation.Any())
                {
                    await _pharmacyInsuranceRepository.AddAsync(new PharmacyInsurance
                    {
                        PharmacyId = realPharmacy.Id,
                        InsuranceCompanyId = (int)unmatched.SourceInsurance.Value
                    });
                }
            }

            unmatched.IsResolved = true;
            unmatched.MatchedPharmacyId = realPharmacy.Id;
            unmatched.UpdatedAt = DateTime.UtcNow;

            _unmatchedPharmacyRepository.Update(unmatched);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Admin eşleştirmesi yapıldı. Karantina ID: {UnmatchedId} -> Hedef Eczane ID: {RealPharmacyId}", matchRequestDto.UnmatchedPharmacyId, matchRequestDto.RealPharmacyId);
        }

        public async Task ApproveAsNewPharmacyAsync(int unmatchedPharmacyId, ApproveAsNewDto dto)
        {
            var unmatched = await _unmatchedPharmacyRepository.GetByIdAsync(unmatchedPharmacyId);
            if (unmatched == null || unmatched.IsResolved)
            {
                throw new Exception("Karantina kaydi bulunamadi veya zaten cozumlenmis.");
            }

            if (!unmatched.DistrictId.HasValue)
            {
                throw new Exception("İlçe bilgisi eksik olduğu için yeni eczane olarak eklenemez. Lütfen manuel eşleştirin.");
            }

            var newPharmacy = new Pharmacy
            {
                Name = !string.IsNullOrWhiteSpace(dto.Name) ? dto.Name : unmatched.ScrapedName,
                Address = !string.IsNullOrWhiteSpace(dto.Address) ? dto.Address : unmatched.ScrapedAddress,
                PhoneNumber = !string.IsNullOrWhiteSpace(dto.PhoneNumber) ? dto.PhoneNumber : (unmatched.ScrapedPhoneNumber ?? string.Empty),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                DistrictId = unmatched.DistrictId.Value,
                IsOnDuty = false,
                CreatedAt = DateTime.UtcNow
            };

            var addedPharmacy = await _pharmacyRepository.AddAsync(newPharmacy);

            if (unmatched.SourceInsurance.HasValue)
            {
                await _pharmacyInsuranceRepository.AddAsync(new PharmacyInsurance
                {
                    Pharmacy = addedPharmacy,
                    InsuranceCompanyId = (int)unmatched.SourceInsurance.Value
                });
            }

            unmatched.IsResolved = true;
            unmatched.MatchedPharmacy = addedPharmacy;
            unmatched.UpdatedAt = DateTime.UtcNow;

            _unmatchedPharmacyRepository.Update(unmatched);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Karantinadaki kayıt yeni eczane olarak onaylandı. Karantina ID: {UnmatchedId} -> Yeni Eczane ID: {PharmacyId}", unmatchedPharmacyId, addedPharmacy.Id);
        }

        public async Task DeleteUnmatchedPharmacyAsync(int unmatchedPharmacyId)
        {
            var unmatched = await _unmatchedPharmacyRepository.GetByIdAsync(unmatchedPharmacyId);
            if (unmatched != null)
            {
                await _unmatchedPharmacyRepository.DeleteByIdAsync(unmatchedPharmacyId);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Karantinadaki eczane admin tarafından silindi. Karantina ID: {UnmatchedId}, İsim: {ScrapedName}", unmatchedPharmacyId, unmatched.ScrapedName);
            }
        }

        public async Task<IEnumerable<PharmacyResponseDto>> GetSuggestionsAsync(int unmatchedPharmacyId)
        {
            var unmatched = await _unmatchedPharmacyRepository.GetByIdAsync(unmatchedPharmacyId);
            if (unmatched == null)
            {
                throw new Exception("Karantina kaydi bulunamadi.");
            }

            IEnumerable<Pharmacy> suggestions = new List<Pharmacy>();

            // 1. Olası eczaneleri getir (ilçe belliyse sadece o ilçe, yoksa il, yoksa hepsi)
            if (unmatched.DistrictId.HasValue)
            {
                var result = await _pharmacyRepository.GetPharmaciesWithDetailsAsync(p => p.DistrictId == unmatched.DistrictId.Value, 1, 1000); 
                suggestions = result.Pharmacies;
            }
            else if (unmatched.CityId.HasValue)
            {
                var result = await _pharmacyRepository.GetPharmaciesWithDetailsAsync(p => p.District.CityId == unmatched.CityId.Value, 1, 2000);
                suggestions = result.Pharmacies;
            }
            else
            {
                var result = await _pharmacyRepository.GetPharmaciesWithDetailsAsync(null, 1, 1000); 
                suggestions = result.Pharmacies;
            }

            string normalizedUnmatchedName = PharmacyProject.Application.Helpers.TextHelper.NormalizeName(unmatched.ScrapedName);
            string normalizedUnmatchedPhone = PharmacyProject.Application.Helpers.TextHelper.NormalizePhone(unmatched.ScrapedPhoneNumber);
            string normalizedUnmatchedAddress = PharmacyProject.Application.Helpers.TextHelper.NormalizeLocationName(unmatched.ScrapedAddress);

            var topSuggestions = suggestions.Select(p => 
            {
                string normalizedDbName = PharmacyProject.Application.Helpers.TextHelper.NormalizeName(p.Name);
                double nameScore = PharmacyProject.Application.Helpers.TextHelper.CalculateSimilarity(normalizedUnmatchedName, normalizedDbName);
                
                double phoneScore = 0;
                if (!string.IsNullOrEmpty(normalizedUnmatchedPhone))
                {
                    string normalizedDbPhone = PharmacyProject.Application.Helpers.TextHelper.NormalizePhone(p.PhoneNumber);
                    if (!string.IsNullOrEmpty(normalizedDbPhone) && normalizedDbPhone == normalizedUnmatchedPhone)
                    {
                        phoneScore = 100.0;
                    }
                }

                double addressScore = 0;
                if (!string.IsNullOrEmpty(normalizedUnmatchedAddress))
                {
                    string normalizedDbAddress = PharmacyProject.Application.Helpers.TextHelper.NormalizeLocationName(p.Address);
                    addressScore = PharmacyProject.Application.Helpers.TextHelper.CalculateSimilarity(normalizedUnmatchedAddress, normalizedDbAddress) * 0.4;
                }

                double totalScore = nameScore + phoneScore + addressScore;

                if (unmatched.DistrictId.HasValue && p.DistrictId == unmatched.DistrictId.Value)
                {
                    totalScore += 20.0;
                }
                else if (unmatched.CityId.HasValue && p.District.CityId == unmatched.CityId.Value)
                {
                    totalScore += 10.0;
                }

                return new { Pharmacy = p, Score = totalScore };
            })
            .OrderByDescending(x => x.Score)
            .Take(100)
            .Select(x => x.Pharmacy)
            .ToList();

            return topSuggestions.Select(p => new PharmacyResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                PhoneNumber = p.PhoneNumber,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                DistrictId = p.DistrictId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }
    }
}
