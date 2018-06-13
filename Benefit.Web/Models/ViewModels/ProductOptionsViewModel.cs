using System.Collections.Generic;
using System.Web.Mvc;
using Benefit.Domain.Models;

namespace Benefit.Web.Models
{
    public class ProductOptionsViewModel
    {
        public Product Product { get; set; }
        public string ProductId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string SellerId { get; set; }
        public List<SelectListItem> Sellers { get; set; }
        
        public ICollection<ProductOption> ProductOptions { get; set; } 
    }
}