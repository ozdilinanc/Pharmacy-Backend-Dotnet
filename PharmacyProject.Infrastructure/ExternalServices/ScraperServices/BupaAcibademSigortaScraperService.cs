using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.ExternalServices.ScraperServices
{
    public class BupaAcibademScraperService : IInsuranceScraperService
    {
        private readonly ILogger<BupaAcibademScraperService> _logger;

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

        public BupaAcibademScraperService(ILogger<BupaAcibademScraperService> logger)
        {
            _logger = logger;
        }

        public string InsuranceName => InsuranceCompanyEnum.BupaAcibademSigorta.ToString();

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

                        string rawJson = await FetchSencardPharmaciesAsync(cityCode);
                        return ParseJsonToPharmacies(rawJson, knownCityName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "BupaAcıbadem (Sencard): {CityCode} plaka kodlu il çekilirken hata oluştu. Hata: {Message}", cityCode, ex.Message);
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

        private async Task<string> FetchSencardPharmaciesAsync(int cityCode)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.sencard.com.tr/admin/api/medicalagency/post/");

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Origin", "https://www.sencard.com.tr");
            request.Headers.Add("Referer", "https://www.sencard.com.tr/sik-kullanilanlar/medikal-hizmet-agi");

            var requestData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("shID", "1"),
                new KeyValuePair<string, string>("productName", "175"),
                new KeyValuePair<string, string>("hcID", "2"), // Eczane
                new KeyValuePair<string, string>("cityID", cityCode.ToString()),
                new KeyValuePair<string, string>("countyID", "0"),
                new KeyValuePair<string, string>("corporateName", ""),
                new KeyValuePair<string, string>("bcList", "0"),
                new KeyValuePair<string, string>("medicalID", "0")
            };

            request.Content = new FormUrlEncodedContent(requestData);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private List<ScrapedPharmacyDto> ParseJsonToPharmacies(string rawJson, string knownCityName)
        {
            var resultList = new List<ScrapedPharmacyDto>();

            using (JsonDocument doc = JsonDocument.Parse(rawJson))
            {

                if (!doc.RootElement.TryGetProperty("ROW", out JsonElement pharmacyArray) || pharmacyArray.ValueKind != JsonValueKind.Array)
                {
                    return resultList;
                }

                foreach (JsonElement element in pharmacyArray.EnumerateArray())
                {
                    string address = element.TryGetProperty("ADDRESS", out var addressProp) ? addressProp.GetString() ?? "" : "";


                    string? extractedDistrict = ExtractDistrictFromAddress(address);

                    var dto = new ScrapedPharmacyDto
                    {
                        InsuranceCompanyName = InsuranceName,
                        Name = element.TryGetProperty("DESCRIPTION", out var descProp) ? descProp.GetString()?.Trim() ?? "" : "",
                        Address = address,
                        PhoneNumber = element.TryGetProperty("PHONE", out var phoneProp) ? phoneProp.GetString() : null,
                        CityName = knownCityName,
                        DistrictName = extractedDistrict,
                        Latitude = null,
                        Longitude = null
                    };

                    resultList.Add(dto);
                }
            }

            return resultList;
        }

        private string? ExtractDistrictFromAddress(string address)
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