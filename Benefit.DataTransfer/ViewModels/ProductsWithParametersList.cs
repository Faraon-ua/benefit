using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProductsWithParametersList
    {
        public ProductsWithParametersList()
        {
            Products = new List<Product>();
            ProductParameters = new List<ProductParameter>();
        }
        public List<Product> Products { get; set; }
        public List<ProductParameter> ProductParameters { get; set; }
        public int ProductsNumber { get; set; }
        public int Page { get; set; }
    }
}
