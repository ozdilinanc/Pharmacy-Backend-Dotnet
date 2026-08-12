using PharmacyProject.Core.Enums;

namespace PharmacyProject.Core.Entities
{
    public class UnmatchedPharmacy : BaseEntity
    {
        public string ScrapedName { get; set; } = string.Empty;
        public string ScrapedAddress { get; set; } = string.Empty;
        public string? ScrapedPhoneNumber { get; set; }
        public InsuranceCompanyEnum? SourceInsurance { get; set; }
        public string DataSource { get; set; } = string.Empty;
        public bool IsResolved { get; set; } = false;
        public int? MatchedPharmacyId { get; set; }
        public Pharmacy? MatchedPharmacy { get; set; }
    }
}