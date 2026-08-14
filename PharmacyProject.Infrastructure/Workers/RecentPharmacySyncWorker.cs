using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Application.Interfaces.Services;

namespace PharmacyProject.Infrastructure.Workers
{
    public class RecentPharmacySyncWorker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecentPharmacySyncWorker> _logger;

        public RecentPharmacySyncWorker(IServiceProvider serviceProvider, ILogger<RecentPharmacySyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SyncRecentPharmaciesAsync()
        {
            try
            {
                _logger.LogInformation("Hangfire: Yeni eczane senkronizasyonu başladı.");

                await using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var nosyApiService = scope.ServiceProvider.GetRequiredService<INosyApiService>();
                    var pharmacyService = scope.ServiceProvider.GetRequiredService<IPharmacyService>();
                    var cityRepo = scope.ServiceProvider.GetRequiredService<PharmacyProject.Application.Interfaces.Repositories.ICityRepository>();
                    var districtRepo = scope.ServiceProvider.GetRequiredService<PharmacyProject.Application.Interfaces.Repositories.IDistrictRepository>();
                    var unmatchedRepo = scope.ServiceProvider.GetRequiredService<PharmacyProject.Application.Interfaces.Repositories.IUnmatchedPharmacyRepository>();
                    var uow = scope.ServiceProvider.GetRequiredService<PharmacyProject.Application.Interfaces.Repositories.IUnitOfWork>();

                    var recentPharmacies = await nosyApiService.GetRecentPharmaciesAsync();
                    var allDbPharmacies = await pharmacyService.GetAllAsync();
                    
                    var allCities = await cityRepo.GetAllAsync();
                    var allDistricts = await districtRepo.GetAllAsync();
                    var cityLookup = allCities.ToDictionary(c => PharmacyProject.Application.Helpers.TextHelper.NormalizeLocationName(c.Name), c => c.Id);
                    var districtLookup = allDistricts.ToDictionary(d => $"{d.CityId}_{PharmacyProject.Application.Helpers.TextHelper.NormalizeLocationName(d.Name)}", d => d.Id);

                    foreach (var apiPharmacy in recentPharmacies)
                    {
                        var normalizedApiPhone = PharmacyProject.Application.Helpers.TextHelper.NormalizePhone(apiPharmacy.Phone);
                        var normalizedApiName = PharmacyProject.Application.Helpers.TextHelper.NormalizeName(apiPharmacy.PharmacyName);

                        bool existsInDb = allDbPharmacies.Any(dbP =>
                            PharmacyProject.Application.Helpers.TextHelper.NormalizePhone(dbP.PhoneNumber) == normalizedApiPhone ||
                            (dbP.Name != null && apiPharmacy.PharmacyName != null && 
                             PharmacyProject.Application.Helpers.TextHelper.NormalizeName(dbP.Name) == normalizedApiName));

                        if (!existsInDb)
                        {
                            int? matchedDistrictId = null;
                            string normCity = PharmacyProject.Application.Helpers.TextHelper.NormalizeLocationName(apiPharmacy.City);
                            string normDist = PharmacyProject.Application.Helpers.TextHelper.NormalizeLocationName(apiPharmacy.District);

                            if (cityLookup.TryGetValue(normCity, out int foundCityId))
                            {
                                if (districtLookup.TryGetValue($"{foundCityId}_{normDist}", out int foundDistrictId))
                                {
                                    matchedDistrictId = foundDistrictId;
                                }
                            }

                            if (matchedDistrictId.HasValue)
                            {
                                var createDto = new CreatePharmacyDto
                                {
                                    Name = apiPharmacy.PharmacyName ?? "Bilinmiyor",
                                    PhoneNumber = apiPharmacy.Phone,
                                    Address = apiPharmacy.Address,
                                    Latitude = apiPharmacy.Latitude,
                                    Longitude = apiPharmacy.Longitude,
                                    DistrictId = matchedDistrictId.Value
                                };

                                await pharmacyService.CreateAsync(createDto);
                                _logger.LogInformation($"Hangfire: Yeni eczane sisteme eklendi: {apiPharmacy.PharmacyName}");
                            }
                            else
                            {
                                _logger.LogWarning($"Hangfire: Yeni eczane için il/ilçe bulunamadı, tabloya ekleniyor! Eczane: {apiPharmacy.PharmacyName}, Şehir: {apiPharmacy.City}, İlçe: {apiPharmacy.District}");
                                var unmatched = new PharmacyProject.Core.Entities.UnmatchedPharmacy
                                {
                                    ScrapedName = apiPharmacy.PharmacyName ?? "Bilinmiyor",
                                    ScrapedPhoneNumber = apiPharmacy.Phone,
                                    ScrapedAddress = $"{apiPharmacy.Address} - Şehir: {apiPharmacy.City}, İlçe: {apiPharmacy.District}",
                                    SourceInsurance = null,
                                    DataSource = "NosyAPI (Yeni)"
                                };
                                await unmatchedRepo.AddAsync(unmatched);
                                await uow.SaveChangesAsync();
                            }
                        }
                    }

                    _logger.LogInformation("Hangfire: Yeni eczane senkronizasyonu tamamlandı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hangfire: RecentPharmacySyncWorker hata fırlattı.");
                throw;
            }
        }
    }
}