using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.ExternalServices.ScraperServices
{
    public class AkSigortaScraperService : IInsuranceScraperService
    {
        private readonly ILogger<AkSigortaScraperService> _logger;

        public AkSigortaScraperService(ILogger<AkSigortaScraperService> logger)
        {
            _logger = logger;
        }

        public string InsuranceName => InsuranceCompanyEnum.Aksigorta.ToString();

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

                        string rawJson = await FetchAkSigortaPharmaciesAsync(cityCode);
                        return ParseJsonToPharmacies(rawJson);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ak Sigorta: {CityCode} plaka kodlu il çekilirken hata oluştu. Hata: {Message}", cityCode, ex.Message);
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

        private async Task<string> FetchAkSigortaPharmaciesAsync(int cityCode)
        {
            string rawJsonPayload = $"{{\"companyId\":\"AKS\",\"cityId\":\"{cityCode}\",\"districtId\":\"\",\"productId\":\"SOS\",\"institutionTypeId\":\"4\",\"networkId\":\"A4\"}}";

            string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawJsonPayload));

            string requestUrl = $"https://api.gratisdev.retter.io/1ld5b7ms3/CALL/CMS/getInstitutions?data={base64Payload}&__isbase64=true";

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Origin", "https://medisa-web.retter.io");
            request.Headers.Add("Referer", "https://medisa-web.retter.io/");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Sunucu Hatası! Status: {(int)response.StatusCode}, Detay: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private List<ScrapedPharmacyDto> ParseJsonToPharmacies(string rawJson)
        {
            var resultList = new List<ScrapedPharmacyDto>();

            using (JsonDocument doc = JsonDocument.Parse(rawJson))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return resultList;
                }

                foreach (JsonElement element in doc.RootElement.EnumerateArray())
                {
                    var dto = new ScrapedPharmacyDto
                    {
                        InsuranceCompanyName = InsuranceName,
                        Name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString()?.Trim() ?? "" : "",
                        Address = element.TryGetProperty("address", out var addressProp) ? addressProp.GetString()?.Trim() ?? "" : "",
                        PhoneNumber = element.TryGetProperty("phone", out var phoneProp) ? phoneProp.GetString() : null,
                        CityName = element.TryGetProperty("cityName", out var cityProp) ? cityProp.GetString()?.Trim() : null,
                        DistrictName = element.TryGetProperty("districtName", out var districtProp) ? districtProp.GetString()?.Trim() : null
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
    }
}