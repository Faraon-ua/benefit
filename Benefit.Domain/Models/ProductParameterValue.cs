using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class ProductParameterValue
    {
        [Key]
        public string Id { get; set; }
        [MaxLength(64)]
        public string ParameterValue { get; set; }
        [MaxLength(64)]
        public string ParameterValueUrl { get; set; }
        public bool IsVerified { get; set; }
        [MaxLength(128)]
        public string AddedBy { get; set; }
        [MaxLength(128)]
        public string ProductParameterId { get; set; }
        public ProductParameter ProductParameter { get; set; }
    }
}
