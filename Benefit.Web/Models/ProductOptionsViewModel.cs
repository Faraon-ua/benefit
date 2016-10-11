using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.Web.Models
{
    public class ProductOptionsViewModel
    {
        public string ProductId { get; set; }
        public string CategoryId { get; set; }
        public string SellerId { get; set; }
        public ICollection<ProductOption> ProductOptions { get; set; } 
    }
}