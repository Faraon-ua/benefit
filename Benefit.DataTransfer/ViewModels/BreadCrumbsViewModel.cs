using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class BreadCrumbsViewModel
    {
        public Dictionary<CategoryVM,List<CategoryVM>> Categories { get; set; }
        public Seller Seller { get; set; }
        public Product Product { get; set; }
        public InfoPage Page { get; set; }
        public bool IsNews { get; set; }
        public bool IsInfoPage { get; set; }
    }
}
