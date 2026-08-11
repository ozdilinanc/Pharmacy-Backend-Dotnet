namespace PharmacyProject.Core.Entities
{
    public class InsuranceCompany : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<PharmacyInsurance> PharmacyInsurances { get; set; } = new List<PharmacyInsurance>();
    }
}
