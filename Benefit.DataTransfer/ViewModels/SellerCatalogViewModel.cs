using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
     public class SellerCatalogViewModel
    {
         public Seller Seller { get; set; }
         public Category SelectedCategory { get; set; }
         public BreadCrumbsViewModel Breadcrumbs { get; set; }
    }
}
