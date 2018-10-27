using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class MappedProductParameter
    {
        [MaxLength(128)]
        [Key, Column(Order = 0)]
        public string SellerId { get; set; }
        public virtual Seller Seller { get; set; }
        [MaxLength(128)]
        [Key, Column(Order = 1)]
        public string ProductParameterId { get; set; }
        public ProductParameter ProductParameter { get; set; }
        public string Name { get; set; }
    }
}
