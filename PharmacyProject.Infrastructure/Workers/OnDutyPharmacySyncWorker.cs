using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using System.Globalization;

namespace PharmacyProject.Infrastructure.Workers
{
    public class OnDutyPharmacySyncWorker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OnDutyPharmacySyncWorker> _logger;

        public OnDutyPharmacySyncWorker(IServiceProvider serviceProvider, ILogger<OnDutyPharmacySyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SyncOnDutyPharmaciesAsync()
        {
            try
            {
                _logger.LogInformation("Hangfire: Nöbetçi eczane senkronizasyonu başladı.");

                await using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var nosyApiService = scope.ServiceProvider.GetRequiredService<INosyApiService>();
                    var pharmacyRepo = scope.ServiceProvider.GetRequiredService<IPharmacyRepository>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var unmatchedRepo = scope.ServiceProvider.GetRequiredService<IUnmatchedPharmacyRepository>();
                    var cityRepo = scope.ServiceProvider.GetRequiredService<ICityRepository>();

                    var currentOnDuties = await pharmacyRepo.GetAllAsync();
                    var activeOnDuties = currentOnDuties.Where(p => p.IsOnDuty).ToList();
                    foreach (var p in activeOnDuties)
                    {
                        p.IsOnDuty = false;
                        pharmacyRepo.Update(p);
                    }
                    await uow.SaveChangesAsync();

                    var allDbPharmacies = await pharmacyRepo.GetAllAsync();
                    var allCities = await cityRepo.GetAllAsync();

                    foreach (var city in allCities)
                    {
                        string cityQuery = city.Slug ?? city.Name;
                        _logger.LogInformation($"Hangfire: {city.Name} ili için nöbetçi eczaneler çekiliyor...");
                        
                        var apiPharmacies = await nosyApiService.GetOnDutyPharmaciesAsync(cityQuery);

                        foreach (var apiPharmacy in apiPharmacies)
                        {
                            var normalizedApiPhone = PharmacyProject.Application.Helpers.TextHelper.NormalizePhone(apiPharmacy.Phone);

                            var matchedDbPharmacy = allDbPharmacies.FirstOrDefault(dbP => PharmacyProject.Application.Helpers.TextHelper.NormalizePhone(dbP.PhoneNumber) == normalizedApiPhone);

                            if (matchedDbPharmacy == null)
                            {
                                var normalizedApiName = PharmacyProject.Application.Helpers.TextHelper.NormalizeName(apiPharmacy.PharmacyName);
                                matchedDbPharmacy = allDbPharmacies.FirstOrDefault(dbP =>
                                    PharmacyProject.Application.Helpers.TextHelper.NormalizeName(dbP.Name) == normalizedApiName &&
                                    dbP.District?.Name.ToLower() == apiPharmacy.District?.ToLower());
                            }

                            if (matchedDbPharmacy != null)
                            {
                                matchedDbPharmacy.IsOnDuty = true;
                                pharmacyRepo.Update(matchedDbPharmacy);
                            }
                            else
                            {
                                _logger.LogWarning($"Eşleşme bulunamadı, tabloya ekleniyor! Eczane: {apiPharmacy.PharmacyName}, İlçe: {apiPharmacy.District}");

                                var unmatched = new UnmatchedPharmacy
                                {
                                    ScrapedName = apiPharmacy.PharmacyName ?? "Bilinmiyor",
                                    ScrapedPhoneNumber = apiPharmacy.Phone,
                                    ScrapedAddress = $"{apiPharmacy.Address} - İlçe: {apiPharmacy.District}",
                                    SourceInsurance = null,
                                    DataSource = "NosyAPI (Nöbetçi)"
                                };

                                await unmatchedRepo.AddAsync(unmatched);
                            }
                        }
                        
                        // Her il sonrası rate limit'i çok zorlamamak için ufak bir bekleme (Opsiyonel, ancak faydalı)
                        await Task.Delay(1000); 
                    }

                    await uow.SaveChangesAsync();
                    _logger.LogInformation("Hangfire: Nöbetçi eczane senkronizasyonu başarıyla tamamlandı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}