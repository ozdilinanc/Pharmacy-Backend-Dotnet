using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;
using System.Globalization;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.ExternalServices.ScraperServices
{
    public class AllianzScraperService : IInsuranceScraperService
    {
        private readonly ILogger<AllianzScraperService> _logger;

        public AllianzScraperService(ILogger<AllianzScraperService> logger)
        {
            _logger = logger;
        }

        public string InsuranceName => InsuranceCompanyEnum.Allianz.ToString();

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

                        string rawJson = await FetchAllianzPharmaciesAsync(cityCode);
                        return ParseJsonToPharmacies(rawJson);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Allianz: {CityCode} plaka kodlu il çekilirken hata oluştu. Hata: {Message}", cityCode, ex.Message);
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

        private async Task<string> FetchAllianzPharmaciesAsync(int cityCode)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://digitall.allianz.com.tr/oneweb-health-module-backend/api/myHealth/search");

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Origin", "https://www.allianz.com.tr");
            request.Headers.Add("Referer", "https://www.allianz.com.tr/");
            request.Headers.Add("Accept", "application/json, text/plain, */*");

            var requestData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("serviceType", "PHARMACY"),
                new KeyValuePair<string, string>("country", "TR"),
                new KeyValuePair<string, string>("city", cityCode.ToString()),
                new KeyValuePair<string, string>("instutionType", "2"),
                new KeyValuePair<string, string>("hospitalType", "1")
            };

            request.Content = new FormUrlEncodedContent(requestData);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private List<ScrapedPharmacyDto> ParseJsonToPharmacies(string rawJson)
        {
            var resultList = new List<ScrapedPharmacyDto>();

            using (JsonDocument doc = JsonDocument.Parse(rawJson))
            {
                if (!doc.RootElement.TryGetProperty("response", out JsonElement pharmacyArray) || pharmacyArray.ValueKind != JsonValueKind.Array)
                {
                    return resultList;
                }

                foreach (JsonElement element in pharmacyArray.EnumerateArray())
                {
                    string address = element.TryGetProperty("address", out var addressProp) ? addressProp.GetString() ?? "" : "";

                    ExtractCityAndDistrict(address, out string? extractedCity, out string? extractedDistrict);

                    var dto = new ScrapedPharmacyDto
                    {
                        InsuranceCompanyName = InsuranceName,
                        Name = element.TryGetProperty("instituteName", out var nameProp) ? nameProp.GetString() ?? "" : "",
                        Address = address,
                        PhoneNumber = element.TryGetProperty("phone", out var phoneProp) ? phoneProp.GetString() : null,
                        CityName = extractedCity,
                        DistrictName = extractedDistrict
                    };

                    if (element.TryGetProperty("latitude", out var latProp))
                    {
                        string? latStr = latProp.GetString();
                        if (double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
                        {
                            dto.Latitude = lat;
                        }
                    }

                    if (element.TryGetProperty("longitude", out var lngProp))
                    {
                        string? lngStr = lngProp.GetString();
                        if (double.TryParse(lngStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lng))
                        {
                            dto.Longitude = lng;
                        }
                    }

                    resultList.Add(dto);
                }
            }

            return resultList;
        }

        private void ExtractCityAndDistrict(string address, out string? city, out string? district)
        {
            city = null;
            district = null;

            if (string.IsNullOrWhiteSpace(address)) return;

            var parts = address.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (parts.Count == 0) return;

            if (parts.Last().Equals("TÜRKİYE", StringComparison.OrdinalIgnoreCase) ||
                parts.Last().Equals("TURKIYE", StringComparison.OrdinalIgnoreCase))
            {
                parts.RemoveAt(parts.Count - 1);
            }

            if (parts.Count == 0) return;

            city = parts.Last();
            parts.RemoveAt(parts.Count - 1);

            if (parts.Count == 0) return;

            if (parts.Last().Length == 5 && parts.Last().All(char.IsDigit))
            {
                parts.RemoveAt(parts.Count - 1);
            }

            if (parts.Count == 0) return;

            district = parts.Last();
        }
    }
}