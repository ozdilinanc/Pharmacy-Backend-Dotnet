using PharmacyProject.Core.Enums;

namespace PharmacyProject.Application.DTOs.Pharmacy
{
    public class UnmatchedPharmacyDto
    {
        public int Id { get; set; }
        public string ScrapedName { get; set; } = string.Empty;
        public string ScrapedAddress { get; set; } = string.Empty;
        public string? ScrapedPhoneNumber { get; set; }
        public InsuranceCompanyEnum? SourceInsurance { get; set; }
        public string DataSource { get; set; } = string.Empty;
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public double? ScrapedLatitude { get; set; }
        public double? ScrapedLongitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
