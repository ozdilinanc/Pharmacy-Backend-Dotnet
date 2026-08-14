using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;
using System.Globalization;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.ExternalServices.ScraperServices
{
    public class AxaSigortaScraperService : IInsuranceScraperService
    {
        private readonly ILogger<AxaSigortaScraperService> _logger;

        public AxaSigortaScraperService(ILogger<AxaSigortaScraperService> logger)
        {
            _logger = logger;
        }

        public string InsuranceName => InsuranceCompanyEnum.AxaSigorta.ToString();

        public async Task<List<ScrapedPharmacyDto>> ScrapeAsync()
        {
            var allPharmacies = new List<ScrapedPharmacyDto>();

            try
            {
                string rawJson = await FetchAxaPharmaciesAsync();
                allPharmacies = ParseJsonToPharmacies(rawJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Axa Sigorta verileri çekilirken hata oluştu. Mesaj: {Message}", ex.Message);
            }

            return allPharmacies;
        }

        private async Task<string> FetchAxaPharmaciesAsync()
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.axasigorta.com.tr/api/axa/contracted/GetHealthServiceTypes?policyType=ozelsaglik");

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", "https://www.axasigorta.com.tr/anlasmali-saglik-kurumlari");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private List<ScrapedPharmacyDto> ParseJsonToPharmacies(string rawJson)
        {
            var resultList = new List<ScrapedPharmacyDto>();

            using (JsonDocument doc = JsonDocument.Parse(rawJson))
            {
                // result -> restResponse -> content hiyerarşisinde güvenle ilerliyoruz
                if (!doc.RootElement.TryGetProperty("result", out var resultElement) ||
                    !resultElement.TryGetProperty("restResponse", out var restResponseElement) ||
                    !restResponseElement.TryGetProperty("content", out JsonElement contentArray) ||
                    contentArray.ValueKind != JsonValueKind.Array)
                {
                    return resultList;
                }

                foreach (JsonElement element in contentArray.EnumerateArray())
                {
                    string kurumTipi = element.TryGetProperty("KURUMTIPI", out var tipProp) ? tipProp.GetString() ?? "" : "";

                    if (!kurumTipi.Contains("ECZANE", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var dto = new ScrapedPharmacyDto
                    {
                        InsuranceCompanyName = InsuranceName,
                        Name = element.TryGetProperty("KURUMADI", out var nameProp) ? nameProp.GetString()?.Trim() ?? "" : "",
                        Address = element.TryGetProperty("ADRES", out var addressProp) ? addressProp.GetString()?.Trim() ?? "" : "",
                        PhoneNumber = element.TryGetProperty("TEL", out var phoneProp) ? phoneProp.GetString() : null,
                        CityName = element.TryGetProperty("SEHIR", out var cityProp) ? cityProp.GetString()?.Trim() : null,
                        DistrictName = element.TryGetProperty("ILCE", out var districtProp) ? districtProp.GetString()?.Trim() : null
                    };

                    if (element.TryGetProperty("ENLEM", out var latProp))
                    {
                        string? latStr = latProp.GetString();
                        if (double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
                        {
                            dto.Latitude = lat;
                        }
                    }

                    if (element.TryGetProperty("BOYLAM", out var lngProp))
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