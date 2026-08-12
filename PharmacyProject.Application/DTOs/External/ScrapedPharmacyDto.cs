namespace PharmacyProject.Application.DTOs.External.Insurance
{
    public class ScrapedPharmacyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? DistrictName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string InsuranceCompanyName { get; set; } = string.Empty;
    }
}