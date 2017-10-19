using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class ProductOption
    {
        public ProductOption()
        {
            ChildProductOptions = new Collection<ProductOption>();
        }
        [Key]
        public string Id { get; set; }
        [MaxLength(64)]
        [Required]
        public string Name { get; set; }
        public bool MultipleSelection { get; set; }
        public bool EditableAmount { get; set; }
        public int Order { get; set; }
        public double PriceGrowth { get; set; }
        [MaxLength(128)]
        public string ParentProductOptionId { get; set; }
        public ProductOption ParentProductOption { get; set; }
        [MaxLength(128)]
        public string BindedProductOptionId { get; set; }
        public ProductOption BindedProductOption { get; set; }
        public ICollection<ProductOption> ChildProductOptions { get; set; }
        public ICollection<ProductOption> BindedProductOptions { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        [MaxLength(128)]
        public string CategoryId { get; set; }
        public Category Category { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }

        [NotMapped]
        public bool Editable { get; set; }
    }
}
