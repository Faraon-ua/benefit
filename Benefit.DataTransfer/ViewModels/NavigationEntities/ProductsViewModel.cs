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
    }
}
