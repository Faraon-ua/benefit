using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Link
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(400)]
        [Required]
        public string Url { get; set; }
        [MaxLength(128)]
        public string ExportImportId { get; set; }
    }
}
