using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class SellersViewModel
    {
        public List<Seller> Items { get; set; }
        public Category Category { get; set; }
        public BreadCrumbsViewModel Breadcrumbs { get; set; }
    }
}
