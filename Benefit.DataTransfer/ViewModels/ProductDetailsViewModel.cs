using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product{ get; set; }
        public string CategoryUrl { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public BreadCrumbsViewModel Breadcrumbs { get; set; }
    }
}
