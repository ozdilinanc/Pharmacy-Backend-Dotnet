using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Application.Interfaces.Repositories;
using System.Globalization;

namespace PharmacyProject.Infrastructure.Workers
{
    public class OnDutyPharmacySyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OnDutyPharmacySyncWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(24);

        public OnDutyPharmacySyncWorker(IServiceProvider serviceProvider, ILogger<OnDutyPharmacySyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Nöbetçi eczane senkronizasyonu başladı.");

                    await using (var scope = _serviceProvider.CreateAsyncScope())
                    {
                        var nosyApiService = scope.ServiceProvider.GetRequiredService<INosyApiService>();
                        var pharmacyRepo = scope.ServiceProvider.GetRequiredService<IPharmacyRepository>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var currentOnDuties = await pharmacyRepo.GetAllAsync();
                        var activeOnDuties = currentOnDuties.Where(p => p.IsOnDuty).ToList();
                        foreach (var p in activeOnDuties)
                        {
                            p.IsOnDuty = false;
                            pharmacyRepo.Update(p);
                        }
                        await uow.SaveChangesAsync();

                        var allDbPharmacies = await pharmacyRepo.GetAllAsync();

                        var apiPharmacies = await nosyApiService.GetOnDutyPharmaciesAsync("ankara");

                        foreach (var apiPharmacy in apiPharmacies)
                        {
                            var normalizedApiPhone = NormalizePhone(apiPharmacy.Phone);

                            var matchedDbPharmacy = allDbPharmacies.FirstOrDefault(dbP => NormalizePhone(dbP.PhoneNumber) == normalizedApiPhone);

                            if (matchedDbPharmacy == null)
                            {
                                var normalizedApiName = NormalizeName(apiPharmacy.PharmacyName);
                                matchedDbPharmacy = allDbPharmacies.FirstOrDefault(dbP =>
                                    NormalizeName(dbP.Name) == normalizedApiName &&
                                    dbP.District?.Name.ToLower() == apiPharmacy.District?.ToLower());
                            }

                            if (matchedDbPharmacy != null)
                            {
                                matchedDbPharmacy.IsOnDuty = true;
                                pharmacyRepo.Update(matchedDbPharmacy);
                            }
                            else
                            {
                                _logger.LogWarning($"Eşleşme bulunamadı! Eczane: {apiPharmacy.PharmacyName}, İlçe: {apiPharmacy.District}");
                            }
                        }

                        await uow.SaveChangesAsync();
                        _logger.LogInformation("Nöbetçi eczane senkronizasyonu tamamlandı.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnDutyPharmacySyncWorker hata fırlattı.");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        private string NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            return digitsOnly.StartsWith("0") ? digitsOnly.Substring(1) : digitsOnly;
        }

        private string NormalizeName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            return name.ToLower(new CultureInfo("tr-TR"))
                       .Replace("eczanesi", "")
                       .Replace("ecz", "")
                       .Replace(" ", "")
                       .Trim();
        }
    }
}