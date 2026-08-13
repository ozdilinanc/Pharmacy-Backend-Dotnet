using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.ExternalServices.ScraperServices
{
    public class AnadoluScraperService : IInsuranceScraperService
    {
        private readonly ILogger<AnadoluScraperService> _logger;

        private readonly string[] _cities = new[]
        {
            "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin", "Aydın", "Balıkesir",
            "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli",
            "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari",
            "Hatay", "Isparta", "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
            "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir",
            "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat",
            "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman",
            "Kırıkkale", "Batman", "Şırnak", "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
        };

        public AnadoluScraperService(ILogger<AnadoluScraperService> logger)
        {
            _logger = logger;
        }

        public string InsuranceName => InsuranceCompanyEnum.AnadoluSigorta.ToString();

        public async Task<List<ScrapedPharmacyDto>> ScrapeAsync()
        {
            var allPharmacies = new List<ScrapedPharmacyDto>();
            var tasks = new List<Task<List<ScrapedPharmacyDto>>>();

            using var throttler = new SemaphoreSlim(3);
            var random = new Random();

            foreach (var city in _cities)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        int delay = random.Next(1000, 3000);
                        await Task.Delay(delay);

                        string upperCityName = city.ToUpper(new CultureInfo("tr-TR"));

                        string rawJson = await FetchAnadoluPharmaciesAsync(upperCityName);
                        return ParseJsonToPharmacies(rawJson, city);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Anadolu Sigorta: {CityName} ili çekilirken hata oluştu. Hata: {Message}", city, ex.Message);
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

        private async Task<string> FetchAnadoluPharmaciesAsync(string cityNameToUpper)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://mscep.anadolusigorta.com.tr/health-claim/api/v1/contracted-health-organisations");

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "*/*");

            request.Headers.Add("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");

            request.Headers.Add("Origin", "https://online.anadolusigorta.com.tr");
            request.Headers.Add("Referer", "https://online.anadolusigorta.com.tr/");
            request.Headers.Add("x-platform", "WEB");
            request.Headers.Add("x-sales-channel", "2000008");

            var payload = new
            {
                city = cityNameToUpper,
                networkTypeCode = "AHN",
                organizationType = "ECZANE",
                queryType = "ANLASMALI_SAGLIK",
                policyType = "OSS",
                networkCodes = Array.Empty<string>(),
                isMember = false
            };

            string jsonBody = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Sunucu Hatası! Status: {(int)response.StatusCode}, Detay: {errorContent}, Gönderilen Şehir: {cityNameToUpper}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private List<ScrapedPharmacyDto> ParseJsonToPharmacies(string rawJson, string knownCityName)
        {
            var resultList = new List<ScrapedPharmacyDto>();

            using (JsonDocument doc = JsonDocument.Parse(rawJson))
            {
                if (!doc.RootElement.TryGetProperty("data", out JsonElement pharmacyArray) || pharmacyArray.ValueKind != JsonValueKind.Array)
                {
                    return resultList;
                }

                foreach (JsonElement element in pharmacyArray.EnumerateArray())
                {
                    string address = element.TryGetProperty("address", out var addressProp) ? addressProp.GetString() ?? "" : "";
                    string? extractedDistrict = ExtractDistrictFromAddress(address, knownCityName);

                    var dto = new ScrapedPharmacyDto
                    {
                        InsuranceCompanyName = InsuranceName,
                        Name = element.TryGetProperty("organizationName", out var nameProp) ? nameProp.GetString()?.Trim() ?? "" : "",
                        Address = address,
                        PhoneNumber = element.TryGetProperty("phone", out var phoneProp) ? phoneProp.GetString() : null,
                        CityName = knownCityName,
                        DistrictName = extractedDistrict
                    };

                    if (element.TryGetProperty("parallel", out var latProp))
                    {
                        string? latStr = latProp.GetString();
                        if (double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
                        {
                            dto.Latitude = lat;
                        }
                    }

                    if (element.TryGetProperty("meridian", out var lngProp))
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

        private string? ExtractDistrictFromAddress(string address, string knownCityName)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var parts = address.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (parts.Count < 2) return null;

            if (parts.Last().Equals(knownCityName, StringComparison.OrdinalIgnoreCase))
            {
                return parts[parts.Count - 2].Trim();
            }

            return null;
        }
    }
}
