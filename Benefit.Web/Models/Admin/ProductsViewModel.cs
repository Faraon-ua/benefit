using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.Web.Models.Admin
{
    public class ProductsViewModel
    {
        public List<Product> Products { get; set; }
        public ProductFilters ProductFilters { get; set; }
    }
}