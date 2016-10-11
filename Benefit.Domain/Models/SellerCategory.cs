using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class SellerCategory
    {
        public double? CustomDiscount { get; set; }
        public bool IsDefault { get; set; }
        [Key, Column(Order = 0)]
        public string SellerId { get; set; }
        public virtual Seller Seller { get; set; }
        [Key, Column(Order = 1)]
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
