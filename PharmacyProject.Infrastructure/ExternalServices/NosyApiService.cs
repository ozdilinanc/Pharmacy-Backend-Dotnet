using Microsoft.Extensions.Configuration;
using PharmacyProject.Application.DTOs.External.NosyApi;
using PharmacyProject.Application.Interfaces.External;
using System.Net.Http.Json;

namespace PharmacyProject.Infrastructure.ExternalServices
{
    public class NosyApiService : INosyApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public NosyApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _apiKey = Environment.GetEnvironmentVariable("NOSY_API_KEY") ?? string.Empty;
            _baseUrl = Environment.GetEnvironmentVariable("NOSY_API_BASE_URL") ?? "https://www.nosyapi.com/apiv2/";

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }

            if (!_httpClient.DefaultRequestHeaders.Contains("X-NSYP"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-NSYP", _apiKey);
            }
        }

        public async Task<List<NosyApiRecentPharmacyDto>> GetRecentPharmaciesAsync()
        {
            var response = await _httpClient.GetAsync("service/pharmaciesv2/recent");
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<NosyApiResponseWrapper<List<NosyApiRecentPharmacyDto>>>();
            return apiResponse?.Data ?? new List<NosyApiRecentPharmacyDto>();
        }

        public async Task<List<NosyApiOnDutyPharmacyDto>> GetOnDutyPharmaciesAsync(string city)
        {
            var endpoint = $"service/pharmacies-on-duty?city={Uri.EscapeDataString(city)}";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<NosyApiResponseWrapper<List<NosyApiOnDutyPharmacyDto>>>();
            return apiResponse?.Data ?? new List<NosyApiOnDutyPharmacyDto>();
        }

        private class NosyApiResponseWrapper<T>
        {
            public string Status { get; set; } = string.Empty;
            public T? Data { get; set; }
        }
    }
}