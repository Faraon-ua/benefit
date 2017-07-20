using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class ProductParameterProductComparer : IEqualityComparer<ProductParameterProduct>
    {
        public bool Equals(ProductParameterProduct x, ProductParameterProduct y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.ProductId == y.ProductId && x.ProductParameterId == y.ProductParameterId &&
                   x.StartValue.ToLower() == y.StartValue.ToLower();
        }

        public int GetHashCode(ProductParameterProduct product)
        {
            if (Object.ReferenceEquals(product, null)) return 0;
            int hashProductId = product.ProductId == null ? 0 : product.ProductId.GetHashCode();
            int hashProductParameterId = product.ProductParameterId.GetHashCode();
            int hashValue = product.StartValue.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductId ^ hashProductParameterId ^ hashValue;
        }

    }
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
        [MaxLength(64)]
        [Key, Column(Order = 2)]
        public string StartValue { get; set; }
        [MaxLength(64)]
        public string StartText { get; set; }
        [MaxLength(64)]
        [Index]
        public string EndValue { get; set; }
        [MaxLength(64)]
        public string EndText { get; set; }
    }
}
