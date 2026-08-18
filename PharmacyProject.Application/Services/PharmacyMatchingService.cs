using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Helpers;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Core.Enums;
using PharmacyProject.Application.Interfaces.Services;

namespace PharmacyProject.Application.Services
{



    public class PharmacyMatchingService : IPharmacyMatchingService
    {
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly IInsuranceRepository _insuranceRepository;
        private readonly IPharmacyInsuranceRepository _pharmacyInsuranceRepository;
        private readonly IUnmatchedPharmacyRepository _unmatchedPharmacyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PharmacyMatchingService> _logger;

        private const double MIN_SIMILARITY_PERCENTAGE = 85.0;
        private const double MAX_DISTANCE_METERS = 50.0;
        private const int BATCH_SIZE = 500;

        public PharmacyMatchingService(
            IPharmacyRepository pharmacyRepository,
            ICityRepository cityRepository,
            IDistrictRepository districtRepository,
            IInsuranceRepository insuranceRepository,
            IPharmacyInsuranceRepository pharmacyInsuranceRepository,
            IUnmatchedPharmacyRepository unmatchedPharmacyRepository,
            IUnitOfWork unitOfWork,
            ILogger<PharmacyMatchingService> logger)
        {
            _pharmacyRepository = pharmacyRepository;
            _cityRepository = cityRepository;
            _districtRepository = districtRepository;
            _insuranceRepository = insuranceRepository;
            _pharmacyInsuranceRepository = pharmacyInsuranceRepository;
            _unmatchedPharmacyRepository = unmatchedPharmacyRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task MatchAndProcessAsync(List<ScrapedPharmacyDto> scrapedPharmacies)
        {
            _logger.LogInformation("Eşleştirme motoru başlatıldı. Gelen veri sayısı: {Count}", scrapedPharmacies.Count);

            var allCities = await _cityRepository.GetAllAsync();
            var allDistricts = await _districtRepository.GetAllAsync();
            var allInsurances = await _insuranceRepository.GetAllAsync();
            var allExistingRelations = await _pharmacyInsuranceRepository.GetAllAsync();

            var cityLookup = allCities.ToDictionary(c => TextHelper.NormalizeLocationName(c.Name), c => c.Id);
            var districtLookup = allDistricts.ToDictionary(d => $"{d.CityId}_{TextHelper.NormalizeLocationName(d.Name)}", d => d.Id);
            var insuranceLookup = allInsurances.ToDictionary(i => i.Name, i => i.Id, StringComparer.OrdinalIgnoreCase);

            var existingRelationsLookup = new HashSet<string>(allExistingRelations.Select(r => $"{r.PharmacyId}_{r.InsuranceCompanyId}"));

            int matchedCount = 0, newPendingCount = 0, relationAddedCount = 0;

            // Batch (Chunk) işlemi ile RAM'in şişmesini (Memory Leak) ve aşırı sorguyu önleme (Paging/Rate Limit)
            var batches = scrapedPharmacies.Chunk(BATCH_SIZE).ToList();

            foreach (var batch in batches)
            {
                var targetCityIds = new HashSet<int>();
                var targetPhones = new HashSet<string>();

                // Bu batch içindeki city id'leri ve telefon numaralarını bulalım
                foreach (var scraped in batch)
                {
                    string normCity = TextHelper.NormalizeLocationName(scraped.CityName);
                    if (cityLookup.TryGetValue(normCity, out int cityId))
                    {
                        targetCityIds.Add(cityId);
                    }
                    
                    var normPhone = TextHelper.NormalizePhone(scraped.PhoneNumber);
                    if (!string.IsNullOrEmpty(normPhone))
                    {
                        targetPhones.Add(normPhone);
                    }
                }

                // Sadece bu batch için gerekli olan eczaneleri (Şehre göre) veritabanından çek (Optimizasyon)
                var dbPharmaciesResult = await _pharmacyRepository.GetPharmaciesWithDetailsAsync(p => p.District != null && targetCityIds.Contains(p.District.CityId), 1, int.MaxValue);
                var dbPharmacies = dbPharmaciesResult.Pharmacies.ToList();
                
                foreach (var scraped in batch)
                {
                    var normalizedScrapedPhone = TextHelper.NormalizePhone(scraped.PhoneNumber);
                    var normalizedScrapedName = TextHelper.NormalizeName(scraped.Name);

                    if (!insuranceLookup.TryGetValue(scraped.InsuranceCompanyName, out int currentInsuranceId))
                    {
                        _logger.LogWarning("Tanımsız Sigorta Firması: {InsuranceName}", scraped.InsuranceCompanyName);
                        continue;
                    }

                    int? scrapedCityId = null;
                    int? scrapedDistrictId = null;
                    string normalizedScrapedCityName = TextHelper.NormalizeLocationName(scraped.CityName);
                    string normalizedScrapedDistrictName = TextHelper.NormalizeLocationName(scraped.DistrictName);

                    if (cityLookup.TryGetValue(normalizedScrapedCityName, out int foundCityId))
                    {
                        scrapedCityId = foundCityId;
                        if (districtLookup.TryGetValue($"{foundCityId}_{normalizedScrapedDistrictName}", out int foundDistrictId))
                        {
                            scrapedDistrictId = foundDistrictId;
                        }
                    }

                    bool isMatched = false;
                    Pharmacy? matchedPharmacy = null;

                    // Bu district'teki eczaneleri tarayalım (Önce telefonla)
                    if (!string.IsNullOrEmpty(normalizedScrapedPhone))
                    {
                        matchedPharmacy = dbPharmacies.FirstOrDefault(dbP => TextHelper.NormalizePhone(dbP.PhoneNumber) == normalizedScrapedPhone);
                        
                        // Eğer bu batch içinde bulamadıysak DB'de direkt arayalım (fallback)
                        if (matchedPharmacy == null)
                        {
                            var possiblePhoneMatches = await _pharmacyRepository.FindAsync(p => p.PhoneNumber != null && p.PhoneNumber.Contains(normalizedScrapedPhone.Length > 7 ? normalizedScrapedPhone.Substring(normalizedScrapedPhone.Length - 7) : normalizedScrapedPhone));
                            matchedPharmacy = possiblePhoneMatches.FirstOrDefault(dbP => TextHelper.NormalizePhone(dbP.PhoneNumber) == normalizedScrapedPhone);
                        }

                        if (matchedPharmacy != null) isMatched = true;
                    }

                    if (!isMatched && scrapedDistrictId.HasValue && !string.IsNullOrEmpty(normalizedScrapedName))
                    {
                        var sameDistrictPharmacies = dbPharmacies.Where(p => p.DistrictId == scrapedDistrictId.Value).ToList();
                        foreach (var dbPharmacy in sameDistrictPharmacies)
                        {
                            var normalizedDbName = TextHelper.NormalizeName(dbPharmacy.Name);
                            double similarity = TextHelper.CalculateSimilarity(normalizedScrapedName, normalizedDbName);
                            if (similarity >= MIN_SIMILARITY_PERCENTAGE)
                            {
                                isMatched = true;
                                matchedPharmacy = dbPharmacy;
                                break;
                            }
                        }
                    }

                    if (!isMatched && scraped.Latitude.HasValue && scraped.Longitude.HasValue)
                    {
                        foreach (var dbPharmacy in dbPharmacies.Where(p => p.Latitude.HasValue && p.Longitude.HasValue))
                        {
                            double distance = GeoHelper.CalculateDistanceInMeters(
                                scraped.Latitude.Value, scraped.Longitude.Value,
                                dbPharmacy.Latitude!.Value, dbPharmacy.Longitude!.Value);

                            if (distance <= MAX_DISTANCE_METERS)
                            {
                                isMatched = true;
                                matchedPharmacy = dbPharmacy;
                                break;
                            }
                        }
                    }

                    if (isMatched && matchedPharmacy != null)
                    {
                        matchedCount++;

                        string relationKey = $"{matchedPharmacy.Id}_{currentInsuranceId}";
                        if (!existingRelationsLookup.Contains(relationKey))
                        {
                            var newRelation = new PharmacyInsurance
                            {
                                PharmacyId = matchedPharmacy.Id,
                                InsuranceCompanyId = currentInsuranceId
                            };
                            await _pharmacyInsuranceRepository.AddAsync(newRelation);

                            existingRelationsLookup.Add(relationKey);
                            relationAddedCount++;
                        }
                    }
                    else
                    {
                        newPendingCount++;

                        InsuranceCompanyEnum? parsedEnum = null;
                        if (Enum.TryParse<InsuranceCompanyEnum>(scraped.InsuranceCompanyName, true, out var insuranceEnum))
                        {
                            parsedEnum = insuranceEnum;
                        }

                        var unmatchedPharmacy = new UnmatchedPharmacy
                        {
                            ScrapedName = scraped.Name ?? "",
                            ScrapedAddress = scraped.Address ?? "",
                            ScrapedPhoneNumber = scraped.PhoneNumber,
                            DataSource = "Scraper",
                            SourceInsurance = parsedEnum,
                            IsResolved = false,
                            MatchedPharmacyId = null,
                            CityId = scrapedCityId,
                            DistrictId = scrapedDistrictId,
                            ScrapedLatitude = scraped.Latitude,
                            ScrapedLongitude = scraped.Longitude
                        };

                        await _unmatchedPharmacyRepository.AddAsync(unmatchedPharmacy);
                    }
                }
                
                // Her 500'lük chunk (batch) sonunda veritabanına kaydet ve RAM'i temizle
                await _unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation("Kayıt İşlemleri Bitti. Başarılı Eşleşme: {Matched}, Yeni Sigorta Bağlantısı: {Relation}, Karantina (Unmatched): {Pending}",
                matchedCount, relationAddedCount, newPendingCount);
        }
    }
}