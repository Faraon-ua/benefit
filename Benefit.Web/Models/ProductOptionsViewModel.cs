using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.Web.Models
{
    public class ProductOptionsViewModel
    {
        public Product Product { get; set; }
        public string CategoryId { get; set; }
        public string SellerId { get; set; }
        public ICollection<ProductOption> ProductOptions { get; set; } 
    }
}