using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;
using System.Text;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.ExternalServices.Scrapers
{
    public class EurekoScraperService : IInsuranceScraperService
    {
        private readonly ILogger<EurekoScraperService> _logger;

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

        public EurekoScraperService(ILogger<EurekoScraperService> logger)
        {
            _logger = logger;
        }

        public string InsuranceName => InsuranceCompanyEnum.EurekoSigorta.ToString();

        public async Task<List<ScrapedPharmacyDto>> ScrapeAsync()
        {
            var allPharmacies = new List<ScrapedPharmacyDto>();
            var tasks = new List<Task<List<ScrapedPharmacyDto>>>();

            using var throttler = new SemaphoreSlim(3);
            var random = new Random();

            for (int i = 1; i <= 81; i++)
            {
                int cityCode = i;
                string knownCityName = _cities[i - 1];

                tasks.Add(Task.Run(async () =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        int delay = random.Next(1000, 3000);
                        await Task.Delay(delay);

                        string rawJson = await FetchEurekoPharmaciesAsync(cityCode);
                        return ParseJsonToPharmacies(rawJson, knownCityName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Eureko: {CityCode} plaka kodlu il çekilirken hata oluştu. Hata: {Message}", cityCode, ex.Message);
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

        private async Task<string> FetchEurekoPharmaciesAsync(int cityCode)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.eurekosigorta.com.tr/api/Provider/PartnerList");

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Referer", "https://www.eurekosigorta.com.tr/anlasmali-kurumlar/anlasmali-saglik-kurumlari");
            request.Headers.Add("Origin", "https://www.eurekosigorta.com.tr");

            request.Headers.Add("x-client-channel", "ESWEB");

            var payload = new
            {
                typeId = 3,
                districtId = "",
                serviceId = "426",
                cityId = cityCode.ToString(),
                medicalId = 2
            };

            string jsonBody = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private List<ScrapedPharmacyDto> ParseJsonToPharmacies(string rawJson, string knownCityName)
        {
            var resultList = new List<ScrapedPharmacyDto>();

            using (JsonDocument doc = JsonDocument.Parse(rawJson))
            {

                if (!doc.RootElement.TryGetProperty("ReturnObject", out JsonElement pharmacyArray) || pharmacyArray.ValueKind != JsonValueKind.Array)
                {
                    return resultList;
                }

                foreach (JsonElement element in pharmacyArray.EnumerateArray())
                {
                    string address = element.TryGetProperty("Address", out var addressProp) ? addressProp.GetString() ?? "" : "";

                    string? extractedDistrict = ExtractDistrictFromEurekoAddress(address);


                    string? phone = null;
                    if (element.TryGetProperty("Phones", out var phoneArray) && phoneArray.ValueKind == JsonValueKind.Array)
                    {
                        phone = phoneArray.EnumerateArray().FirstOrDefault().GetString();
                    }

                    var dto = new ScrapedPharmacyDto
                    {
                        InsuranceCompanyName = InsuranceName,
                        Name = element.TryGetProperty("Title", out var titleProp) ? titleProp.GetString() ?? "" : "",
                        Address = address,
                        PhoneNumber = phone,
                        CityName = knownCityName,
                        DistrictName = extractedDistrict
                    };

                    if (element.TryGetProperty("GeoLat", out var latProp) && latProp.TryGetDouble(out double lat))
                    {
                        dto.Latitude = lat;
                    }

                    if (element.TryGetProperty("GeoLon", out var lngProp) && lngProp.TryGetDouble(out double lng))
                    {
                        dto.Longitude = lng;
                    }

                    resultList.Add(dto);
                }
            }

            return resultList;
        }

        private string? ExtractDistrictFromEurekoAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var lastWord = address.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

            if (lastWord != null && lastWord.Contains("/"))
            {
                var parts = lastWord.Split('/');
                if (parts.Length > 0)
                {
                    return parts[0].Trim();
                }
            }

            return null;
        }
    }
}