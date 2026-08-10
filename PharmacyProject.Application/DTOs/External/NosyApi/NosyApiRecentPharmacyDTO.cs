using System.Text.Json.Serialization;

namespace PharmacyProject.Application.DTOs.External.NosyApi;

public class NosyApiRecentPharmacyDto
{
    [JsonPropertyName("pharmacyID")]
    public int PharmacyId { get; set; }

    [JsonPropertyName("pharmacyName")]
    public string PharmacyName { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("district")]
    public string District { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}