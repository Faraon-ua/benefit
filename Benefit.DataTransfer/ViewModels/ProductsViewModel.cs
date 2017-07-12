using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProductsViewModel
    {
        public List<Product> Items { get; set; }
        public Category Category { get; set; }
        public Seller Seller { get; set; }
        public ICollection<ProductParameter> ProductParameters { get; set; }
        
        public BreadCrumbsViewModel Breadcrumbs { get; set; }
    }
}
