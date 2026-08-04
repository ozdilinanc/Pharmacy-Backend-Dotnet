namespace PharmacyProject.Core.Entities
{
    public class District : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int CityId { get; set; }
        public City City { get; set; } = null!;
    }
}
