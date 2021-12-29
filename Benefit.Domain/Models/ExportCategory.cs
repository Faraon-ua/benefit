using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class ExportCategory
    {
        [Key, Column(Order = 0)]
        public string CategoryId { get; set; }
        public Category Category { get; set; }
        [Key, Column(Order = 1)]
        public string ExportId { get; set; }
        public ExportImport Export { get; set; }
        [Required]
        [MaxLength(64)]
        public string Name { get; set; }
        [MaxLength(64)]
        public string ExternalId { get; set; }
    }
}
