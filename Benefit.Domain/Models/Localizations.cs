using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class Localization
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(128)]
        public string ResourceId { get; set; }
        [Required]
        [MaxLength(128)]
        public string ResourceType { get; set; }
        [Required]
        [MaxLength(32)]
        public string ResourceField { get; set; }
        [Required]
        public string ResourceValue { get; set; }
        [Required]
        [MaxLength(4)]
        public string LanguageCode { get; set; }
        [NotMapped]
        public string ResourceOriginalValue { get; set; }
    }
}
