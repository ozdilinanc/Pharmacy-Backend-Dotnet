using PharmacyProject.Core.Enums;

namespace PharmacyProject.Core.Entities
{
    public class Pharmacy : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public int DistrictId { get; set; }
        public District District { get; set; } = null!;

        public bool IsOnDuty { get; set; } = false;

        public ICollection<PharmacyInsurance> PharmacyInsurances { get; set; } = new List<PharmacyInsurance>();

        public List<InsuranceCompanyEnum> SupportedInsurances { get; set; } = new();
    }
}
