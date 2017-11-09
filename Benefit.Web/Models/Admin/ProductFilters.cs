using System.Collections.Generic;
using System.Web.Mvc;
using Benefit.Domain.Models.Enums;

namespace Benefit.Web.Models.Admin
{
    public class ProductFilterValues
    {
        public string Search { get; set; }
        public string CategoryId { get; set; }
        public string SellerId { get; set; }
        public ProductSortOption? Sorting{ get; set; }
        public bool IsAvailable { get; set; }
        public bool HasImage { get; set; }
        public bool IsActive { get; set; }
        public int Page { get; set; }
        public bool? HasParameters { get; set; }
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
        public IEnumerable<SelectListItem> HasParameters { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsActive{ get; set; }
        public bool HasImage { get; set; }
        public string Search { get; set; }
        public int PagesCount { get; set; }
    }
}