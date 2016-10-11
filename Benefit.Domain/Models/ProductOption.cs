using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class ProductOption
    {
        [Key]
        public string Id { get; set; }
        [MaxLength(64)]
        [Required]
        public string Name { get; set; }
        public bool MultipleSelection { get; set; }
        public double PriceGrowth { get; set; }
        [MaxLength(128)]
        public string ParentProductOptionId { get; set; }
        public ProductOption ParentProductOption { get; set; }
        public ICollection<ProductOption> ChildProductOptions { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        [MaxLength(128)]
        public string CategoryId { get; set; }
        public Category Category { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
