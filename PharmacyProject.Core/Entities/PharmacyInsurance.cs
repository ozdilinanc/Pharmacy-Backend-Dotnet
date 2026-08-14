namespace PharmacyProject.Core.Entities
{
    public class PharmacyInsurance : BaseEntity
    {
        public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = null!;
        public int InsuranceCompanyId { get; set; }
        public InsuranceCompany InsuranceCompany { get; set; } = null!;
    }
}
