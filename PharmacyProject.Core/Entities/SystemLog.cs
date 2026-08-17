using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyProject.Core.Entities
{
    public class SystemLog : BaseEntity
    {
        public string? Message { get; set; }
        public string? MessageTemplate { get; set; }
        public string? Level { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Exception { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string? Properties { get; set; } // Serilog log context data in JSON format
    }
}
