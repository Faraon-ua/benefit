using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class ProductParameterValueComparer : IEqualityComparer<ProductParameterValue>
    {
        public bool Equals(ProductParameterValue x, ProductParameterValue y)
        {
            return x.ParameterValueUrl == y.ParameterValueUrl;
        }

        public int GetHashCode(ProductParameterValue obj)
        {
            return obj.ParameterValueUrl.GetHashCode();
        }
    } 
    public class ProductParameterValue
    {
        [Key]
        public string Id { get; set; }
        [MaxLength(64)]
        [Required]
        public string ParameterValue { get; set; }
        [MaxLength(64)]
        [Required]
        public string ParameterValueUrl { get; set; }
        public bool IsVerified { get; set; }
        [MaxLength(128)]
        public string AddedBy { get; set; }
        public int Order { get; set; }
        [MaxLength(128)]
        public string ProductParameterId { get; set; }
        public ProductParameter ProductParameter { get; set; }
    }
}
