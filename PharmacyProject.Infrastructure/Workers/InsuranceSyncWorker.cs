using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PharmacyProject.Application.Interfaces.External;

namespace PharmacyProject.Infrastructure.Workers
{
    public class InsuranceSyncWorker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InsuranceSyncWorker> _logger;

        public InsuranceSyncWorker(IServiceProvider serviceProvider, ILogger<InsuranceSyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SyncInsuranceAsync(string insuranceName)
        {
            try
            {
                await using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var scrapers = scope.ServiceProvider.GetRequiredService<IEnumerable<IInsuranceScraperService>>();
                    var scraper = scrapers.FirstOrDefault(s => s.InsuranceName == insuranceName);
                    
                    if (scraper == null)
                    {
                        _logger.LogWarning("{Insurance} için scraper servisi bulunamadı!", insuranceName);
                        return;
                    }

                    _logger.LogInformation("{Insurance} için kazıma işlemi Hangfire tarafından başlatıldı...", scraper.InsuranceName);

                    var scrapedData = await scraper.ScrapeAsync();

                    if (scrapedData != null && scrapedData.Any())
                    {
                        _logger.LogInformation("{Insurance} kazıma tamamlandı. Çekilen Veri: {Count}. Eşleştirme motoruna gönderiliyor...", scraper.InsuranceName, scrapedData.Count);

                        var matchingService = scope.ServiceProvider.GetRequiredService<PharmacyProject.Application.Interfaces.Services.IPharmacyMatchingService>();
                        await matchingService.MatchAndProcessAsync(scrapedData);

                        _logger.LogInformation("{Insurance} eşleştirme ve veritabanı kayıt işlemi başarıyla bitti!", scraper.InsuranceName);
                    }
                    else
                    {
                        _logger.LogWarning("{Insurance} servisinden hiç veri dönmedi veya site 404 verdi!", scraper.InsuranceName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sigorta senkronizasyonu (Scrape & Match) sırasında beklenmeyen bir hata oluştu.");
            }
        }
    }
}