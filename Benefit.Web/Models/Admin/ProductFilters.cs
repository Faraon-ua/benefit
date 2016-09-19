using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace Benefit.Web.Models.Admin
{
    public enum ProductSortOption
    {
        [Description("Назва &#8593;")]
        NameAsc,
        [Description("Назва &#8595;")]
        NameDesc,
        [Description("Код товару &#8593;")]
        SKUAsc,
        [Description("Код товару &#8595;")]
        SKUDesc,
        [Description("Ціна &#8593;")]
        PriceAsc,
        [Description("Ціна &#8595;")]
        PriceDesc
    }
    public class ProductFilterValues
    {
        public string Search { get; set; }
        public string CategoryId { get; set; }
        public string SellerId { get; set; }
        public ProductSortOption? Sorting{ get; set; }
        public bool IsAvailable { get; set; }

        public bool HasValues
        {
            get { return (Search != null || CategoryId != null || SellerId != null); }
        }
    }
    public class ProductFilters
    {
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Sellers { get; set; }
        public IEnumerable<SelectListItem> Sorting { get; set; }
        public bool IsAvailable { get; set; }
        public string Search { get; set; }
    }
}