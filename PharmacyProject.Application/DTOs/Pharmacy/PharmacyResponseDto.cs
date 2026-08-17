namespace PharmacyProject.Application.DTOs.Pharmacy
{
    public class SupportedInsuranceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PharmacyResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public List<SupportedInsuranceDto> SupportedInsurances { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
