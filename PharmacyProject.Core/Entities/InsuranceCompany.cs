namespace PharmacyProject.Core.Entities
{
    public class InsuranceCompany : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<PharmacyInsurance> PharmacyInsurances { get; set; };
    }
}
