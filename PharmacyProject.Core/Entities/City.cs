namespace PharmacyProject.Core.Entities
{
    public class City : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}
