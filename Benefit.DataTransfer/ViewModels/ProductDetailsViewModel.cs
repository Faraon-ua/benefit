using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductDetailsViewModel()
        {
            ProductOptions = new List<ProductOption>();
        }
        public Product Product{ get; set; }
        public string CategoryUrl { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public BreadCrumbsViewModel Breadcrumbs { get; set; }
        public bool CanReview { get; set; }
        public List<Product> RelatedProducts { get; set; }
        public double DiscountPercent { get; set; }
    }
}
