using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProductsWithParametersList
    {
        public List<Product> Products { get; set; }
        public List<ProductParameterProduct> ProductParameterProducts { get; set; }
    }
}
