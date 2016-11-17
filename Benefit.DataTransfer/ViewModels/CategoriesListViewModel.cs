using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class CategoriesListViewModel
    {
        public string ParentName { get; set; }
        public string SellerUrlName { get; set; }
        public List<Category> Items { get; set; } 
    }
}
