using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProductsWithParametersList
    {
        public ProductsWithParametersList()
        {
            Products = new List<Product>();
            ProductParameters = new List<string>();
        }
        public List<Product> Products { get; set; }
        public List<string> ProductParameters { get; set; }
    }
}
