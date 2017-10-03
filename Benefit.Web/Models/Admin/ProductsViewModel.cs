using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.Web.Models.Admin
{
    public class ProductsViewModel
    {
        public int TotalProductsCount { get; set; }
        public List<Product> Products { get; set; }
        public ProductFilters ProductFilters { get; set; }
    }
}