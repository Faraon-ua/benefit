using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class ProductParameterProduct
    {
        [Key, Column(Order = 0)]
        [MaxLength(128)]
        public string ProductParameterId { get; set; }
        public ProductParameter ProductParameter { get; set; }
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        public int? Amount { get; set; }
        public string StartValue { get; set; }
        public string EndValue { get; set; }
    }
}
