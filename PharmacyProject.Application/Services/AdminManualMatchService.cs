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

            if (unmatched.CityId.HasValue)
            {
                var result = await _pharmacyRepository.GetPharmaciesWithDetailsAsync(p => p.District.CityId == unmatched.CityId.Value, 1, int.MaxValue);
                suggestions = result.Pharmacies;
            }
            else
            {
                var result = await _pharmacyRepository.GetPharmaciesWithDetailsAsync(null, 1, int.MaxValue); // Fallback to all (limit is applied below)
                suggestions = result.Pharmacies;
            }

            string normalizedUnmatchedName = PharmacyProject.Application.Helpers.TextHelper.NormalizeName(unmatched.ScrapedName);

            var topSuggestions = suggestions.Select(p => 
            {
                string normalizedDbName = PharmacyProject.Application.Helpers.TextHelper.NormalizeName(p.Name);
                double score = PharmacyProject.Application.Helpers.TextHelper.CalculateSimilarity(normalizedUnmatchedName, normalizedDbName);
                
                if (unmatched.DistrictId.HasValue && p.DistrictId == unmatched.DistrictId.Value)
                {
                    score += 20.0;
                }

                return new { Pharmacy = p, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .Take(20)
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
