using System.Collections.Generic;
using System.Web.Mvc;

namespace Benefit.Web.Models.Admin
{
    public class ProductFilterValues
    {
        public string Search { get; set; }
        public string CategoryId { get; set; }
        public string SellerId { get; set; }
    }
    public class ProductFilters
    {
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Sellers { get; set; }
    }
}