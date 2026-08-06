namespace PharmacyProject.Application.DTOs.Pharmacy
{
    public class UpdatePharmacyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int DistrictId { get; set; }
    }
}
