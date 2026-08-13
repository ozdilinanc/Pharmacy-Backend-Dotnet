using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Helpers;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Application.Interfaces.Services;

namespace PharmacyProject.Application.Services
{
    public interface IPharmacyMatchingService
    {
        Task MatchAndProcessAsync(List<ScrapedPharmacyDto> scrapedPharmacies);
    }

    public class PharmacyMatchingService : IPharmacyMatchingService
    {
        private readonly IPharmacyService _pharmacyService;
        private readonly ICityRepository _cityRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly ILogger<PharmacyMatchingService> _logger;

        private const double MIN_SIMILARITY_PERCENTAGE = 85.0;
        private const double MAX_DISTANCE_METERS = 50.0;

        public PharmacyMatchingService(
            IPharmacyService pharmacyService,
            ICityRepository cityRepository,
            IDistrictRepository districtRepository,
            ILogger<PharmacyMatchingService> logger)
        {
            _pharmacyService = pharmacyService;
            _cityRepository = cityRepository;
            _districtRepository = districtRepository;
            _logger = logger;
        }

        public async Task MatchAndProcessAsync(List<ScrapedPharmacyDto> scrapedPharmacies)
        {
            _logger.LogInformation("Eşleştirme motoru başlatıldı. Gelen veri sayısı: {Count}", scrapedPharmacies.Count);

            var allDbPharmacies = await _pharmacyService.GetAllAsync();
            var allCities = await _cityRepository.GetAllAsync();
            var allDistricts = await _districtRepository.GetAllAsync();

            var cityLookup = allCities.ToDictionary(
                c => TextHelper.NormalizeLocationName(c.Name),
                c => c.Id);


            var districtLookup = allDistricts.ToDictionary(
                d => $"{d.CityId}_{TextHelper.NormalizeLocationName(d.Name)}",
                d => d.Id);

            int matchedCount = 0, newPendingCount = 0;

            foreach (var scraped in scrapedPharmacies)
            {
                var normalizedScrapedPhone = TextHelper.NormalizePhone(scraped.PhoneNumber);
                var normalizedScrapedName = TextHelper.NormalizeName(scraped.Name);

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

                if (!string.IsNullOrEmpty(normalizedScrapedPhone))
                {
                    var phoneMatch = allDbPharmacies.FirstOrDefault(dbP => TextHelper.NormalizePhone(dbP.PhoneNumber) == normalizedScrapedPhone);
                    if (phoneMatch != null)
                    {
                        isMatched = true;
                        matchedCount++;
                        // TODO: Bulunan phoneMatch eczanesine Sigorta ID'si eklenecek
                        continue;
                    }
                }

                if (!isMatched && scrapedDistrictId.HasValue && !string.IsNullOrEmpty(normalizedScrapedName))
                {
                    var sameDistrictPharmacies = allDbPharmacies.Where(p => p.DistrictId == scrapedDistrictId.Value).ToList();

                    foreach (var dbPharmacy in sameDistrictPharmacies)
                    {
                        var normalizedDbName = TextHelper.NormalizeName(dbPharmacy.Name);
                        double similarity = TextHelper.CalculateSimilarity(normalizedScrapedName, normalizedDbName);

                        if (similarity >= MIN_SIMILARITY_PERCENTAGE)
                        {
                            isMatched = true;
                            matchedCount++;
                            // TODO: Bulunan dbPharmacy eczanesine Sigorta ID'si eklenecek
                            break;
                        }
                    }
                    if (isMatched) continue;
                }

                if (!isMatched && scraped.Latitude.HasValue && scraped.Longitude.HasValue)
                {
                    foreach (var dbPharmacy in allDbPharmacies.Where(p => p.Latitude.HasValue && p.Longitude.HasValue))
                    {
                        double distance = GeoHelper.CalculateDistanceInMeters(
                            scraped.Latitude.Value, scraped.Longitude.Value,
                            dbPharmacy.Latitude.Value, dbPharmacy.Longitude.Value);

                        if (distance <= MAX_DISTANCE_METERS)
                        {
                            isMatched = true;
                            matchedCount++;
                            break;
                        }
                    }
                }

                if (!isMatched)
                {
                    newPendingCount++;
                    // TODO: UnmatchedPharmacy tablosuna veya Pending duruma kaydedilecek
                }
            }

            _logger.LogInformation("Eşleştirme Tamamlandı. Başarılı Eşleşme: {Matched}, Karantinaya Alınan: {Pending}", matchedCount, newPendingCount);
        }
    }
}