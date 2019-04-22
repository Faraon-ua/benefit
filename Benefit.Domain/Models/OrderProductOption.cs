using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class OrderProductOption
    {
        [Key, Column(Order = 0)]
        [MaxLength(128)]
        public string ProductOptionId { get; set; }
        [MaxLength(128)]
        [Key, Column(Order = 1)]
        public string OrderProductId { get; set; }
        public string ProductOptionName { get; set; }
        public double ProductOptionPriceGrowth { get; set; }
        public int Amount { get; set; }

        [NotMapped]
        public bool EditableAmount { get; set; }
    }
}
