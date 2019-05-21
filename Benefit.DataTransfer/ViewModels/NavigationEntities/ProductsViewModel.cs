using System.Collections.Generic;
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels.NavigationEntities
{
    public class ProductsViewModel : NavigationEntitiesViewModel<Product>
    {
        public ProductsViewModel()
        {
            ProductParameters = new List<ProductParameter>();
        }
        public ICollection<ProductParameter> ProductParameters { get; set; }
        public Dictionary<string, double> CategoryToSellerDiscountPercent { get; set; }
        public int Page { get; set; }
        public int PagesCount { get; set; }
        public bool IsFavorites { get; set; }
    }
}
