using Microsoft.Extensions.Logging;
using PharmacyProject.Application.DTOs.External.Insurance;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Core.Enums;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.ExternalServices.ScraperServices
{
    public class MapfreSigortaScraperService : IInsuranceScraperService
    {
        private readonly ILogger<MapfreSigortaScraperService> _logger;


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

        public MapfreSigortaScraperService(ILogger<MapfreSigortaScraperService> logger)
        {
            _logger = logger;
        }

        public string InsuranceName => InsuranceCompanyEnum.MapfreSigorta.ToString();

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

                        string rawJson = await FetchMapfrePharmaciesAsync(city);
                        return ParseJsonToPharmacies(rawJson);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Mapfre: {CityName} ili çekilirken hata oluştu. Hata: {Message}", city, ex.Message);
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

        private async Task<string> FetchMapfrePharmaciesAsync(string cityName)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://form.mapfre.com.tr/ComeRoundForms/biws/api/searchContractedInstitutions");

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Origin", "https://form.mapfre.com.tr");
            request.Headers.Add("Referer", "https://form.mapfre.com.tr/iletisim/formlar/");

            var payload = new
            {
                cityId = cityName,
                districtId = "",
                networkTypeCode = "65",
                institutionType = "ECZANE",
                expertiseType = ""
            };

            string jsonBody = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private List<ScrapedPharmacyDto> ParseJsonToPharmacies(string rawJson)
        {
            var resultList = new List<ScrapedPharmacyDto>();

            using (JsonDocument doc = JsonDocument.Parse(rawJson))
            {
                if (!doc.RootElement.TryGetProperty("RecordList", out JsonElement pharmacyArray) || pharmacyArray.ValueKind != JsonValueKind.Array)
                {
                    return resultList;
                }

                foreach (JsonElement element in pharmacyArray.EnumerateArray())
                {
                    var dto = new ScrapedPharmacyDto
                    {
                        InsuranceCompanyName = InsuranceName,
                        Name = element.TryGetProperty("recordname", out var nameProp) ? nameProp.GetString() ?? "" : "",
                        Address = element.TryGetProperty("address", out var addressProp) ? addressProp.GetString() ?? "" : "",
                        PhoneNumber = element.TryGetProperty("telephone", out var phoneProp) ? phoneProp.GetString() : null,
                        CityName = element.TryGetProperty("iladi", out var cityProp) ? cityProp.GetString() : null,
                        DistrictName = element.TryGetProperty("ilceadi", out var districtProp) ? districtProp.GetString() : null
                    };

                    if (element.TryGetProperty("centerLocation", out var locationElement))
                    {
                        if (locationElement.TryGetProperty("x_coord", out var latProp))
                        {
                            string? latStr = latProp.GetString();
                            if (double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
                            {
                                dto.Latitude = lat;
                            }
                        }

                        if (locationElement.TryGetProperty("y_coord", out var lngProp))
                        {
                            string? lngStr = lngProp.GetString();
                            if (double.TryParse(lngStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lng))
                            {
                                dto.Longitude = lng;
                            }
                        }
                    }

                    resultList.Add(dto);
                }
            }

            return resultList;
        }
    }
}