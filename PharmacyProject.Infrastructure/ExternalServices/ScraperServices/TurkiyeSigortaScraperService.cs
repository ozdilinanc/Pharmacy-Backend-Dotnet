using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;
using System.Text;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.ExternalServices.ScraperServices
{
    public class TurkiyeSigortaScraperService : IInsuranceScraperService
    {
        private readonly ILogger<TurkiyeSigortaScraperService> _logger;

        public TurkiyeSigortaScraperService(ILogger<TurkiyeSigortaScraperService> logger)
        {
            _logger = logger;
        }

        public string InsuranceName => InsuranceCompanyEnum.TurkiyeSigorta.ToString();

        public async Task<List<ScrapedPharmacyDto>> ScrapeAsync()
        {
            var allPharmacies = new List<ScrapedPharmacyDto>();
            var tasks = new List<Task<List<ScrapedPharmacyDto>>>();

            using var throttler = new SemaphoreSlim(3);
            var random = new Random();

            for (int i = 1; i <= 81; i++)
            {
                int cityCode = i;

                tasks.Add(Task.Run(async () =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        int delay = random.Next(1000, 3000);
                        await Task.Delay(delay);

                        string rawData = await FetchTurkiyeSigortaPharmaciesAsync(cityCode);
                        return ParseRscToPharmacies(rawData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Türkiye Sigorta: {CityCode} plaka kodlu il çekilirken hata oluştu. Hata: {Message}", cityCode, ex.Message);

                        return new List<ScrapedPharmacyDto>();
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                allPharmacies.AddRange(result);
            }

            return allPharmacies;
        }

        private async Task<string> FetchTurkiyeSigortaPharmaciesAsync(int cityCode)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.turkiyesigorta.com.tr/yardim-merkezi/iletisim/anlasmali-kurum-basvuru/anlasmali-saglik-kurumlari");

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "text/x-component");
            request.Headers.Add("next-action", "7f055450a5104c968987cfa68442f2bd5be2686fde");
            request.Headers.Add("Origin", "https://www.turkiyesigorta.com.tr");
            request.Headers.Add("Referer", "https://www.turkiyesigorta.com.tr/yardim-merkezi/iletisim/anlasmali-kurum-basvuru/anlasmali-saglik-kurumlari");

            string jsonBody = $"[{{\"urunKodu\":\"85259\",\"networkId\":\"748\",\"ilId\":\"{cityCode}\",\"kurumTipi\":\"22\",\"ilceId\":\"$undefined\"}}]";

            request.Content = new StringContent(jsonBody, Encoding.UTF8, "text/plain");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private List<ScrapedPharmacyDto> ParseRscToPharmacies(string rawData)
        {
            var resultList = new List<ScrapedPharmacyDto>();

            var lines = rawData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string? jsonArrayString = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("1: [") || line.StartsWith("1:["))
                {
                    jsonArrayString = line.Substring(line.IndexOf('[')).Trim();
                    break;
                }
                else if (line.Contains("[{\"ad\":"))
                {
                    int startIndex = line.IndexOf('[');
                    jsonArrayString = line.Substring(startIndex).Trim();
                    break;
                }
            }

            if (string.IsNullOrEmpty(jsonArrayString)) return resultList;

            using (JsonDocument doc = JsonDocument.Parse(jsonArrayString))
            {
                foreach (JsonElement element in doc.RootElement.EnumerateArray())
                {
                    var dto = new ScrapedPharmacyDto
                    {
                        InsuranceCompanyName = InsuranceName,
                        Name = element.TryGetProperty("ad", out var nameProp) ? nameProp.GetString() ?? "" : "",
                        Address = element.TryGetProperty("adres", out var addressProp) ? addressProp.GetString() ?? "" : "",
                        PhoneNumber = element.TryGetProperty("telefon", out var phoneProp) ? phoneProp.GetString() : null,
                        CityName = element.TryGetProperty("il", out var cityProp) ? cityProp.GetString() : null,
                        DistrictName = element.TryGetProperty("ilce", out var districtProp) ? districtProp.GetString() : null,
                        Latitude = null,
                        Longitude = null
                    };

                    resultList.Add(dto);
                }
            }

            return resultList;
        }
    }
}