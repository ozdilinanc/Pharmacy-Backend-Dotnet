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

                    var recentPharmacies = await nosyApiService.GetRecentPharmaciesAsync();
                    var allDbPharmacies = await pharmacyService.GetAllAsync();

                    foreach (var apiPharmacy in recentPharmacies)
                    {
                        var normalizedApiPhone = NormalizePhone(apiPharmacy.Phone);

                        bool existsInDb = allDbPharmacies.Any(dbP =>
                            NormalizePhone(dbP.PhoneNumber) == normalizedApiPhone ||
                            (dbP.Name != null && apiPharmacy.PharmacyName != null && dbP.Name.Contains(apiPharmacy.PharmacyName.Replace("Eczanesi", "").Trim())));

                        if (!existsInDb)
                        {
                            var createDto = new CreatePharmacyDto
                            {
                                Name = apiPharmacy.PharmacyName ?? "Bilinmiyor",
                                PhoneNumber = apiPharmacy.Phone,
                                Address = apiPharmacy.Address,
                                Latitude = apiPharmacy.Latitude,
                                Longitude = apiPharmacy.Longitude,
                                DistrictId = 1 // Şimdilik varsayılan 1
                            };

                            await pharmacyService.CreateAsync(createDto);
                            _logger.LogInformation($"Hangfire: Yeni eczane sisteme eklendi: {apiPharmacy.PharmacyName}");
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

        private string NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            return digitsOnly.StartsWith("0") ? digitsOnly.Substring(1) : digitsOnly;
        }
    }
}