using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class SellerCategory
    {
        public double? CustomDiscount { get; set; }
        public bool RootDisplay { get; set; }
        public bool IsDefault { get; set; }
        [Key, Column(Order = 0)]
        public string SellerId { get; set; }
        public virtual Seller Seller { get; set; }
        [Key, Column(Order = 1)]
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }

    public class SellerCategoryComparer : IEqualityComparer<SellerCategory>
    {
        public bool Equals(SellerCategory x, SellerCategory y)
        {
            if (x.CategoryId == y.CategoryId && x.SellerId == y.SellerId)
            {
                return true;
            }
            return false;
        }
        public int GetHashCode(SellerCategory codeh)
        {
            return (codeh.CategoryId + codeh.SellerId).GetHashCode();
        }
    }
}
