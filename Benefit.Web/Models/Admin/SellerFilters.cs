using System.Collections.Generic;
using System.Web.Mvc;

namespace Benefit.Web.Models.Admin
{
    public class SellerFilterValues
    {
        public string Search { get; set; }
        public string DateRange { get; set; }
        public string CategoryId { get; set; }
        public int? TotalDiscountPercent { get; set; }
        public double? UserDiscountPercent { get; set; }
        public bool BenefitCard { get; set; }
        public bool Ecommerce { get; set; }
    }
    public class SellerFilterOptions
    {
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> PointRatio { get; set; }
        public IEnumerable<SelectListItem> TotalDiscountPercent { get; set; }
        public IEnumerable<SelectListItem> UserDiscountPercent { get; set; }
        public bool BenefitCard { get; set; }
        public bool Ecommerce { get; set; }
    }
}