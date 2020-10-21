using System.Collections.Generic;
using System.Web.Mvc;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models;
using Benefit.Common.Constants;

namespace Benefit.Web.Models.Admin
{
    public class ProductFilterValues
    {
        public ProductFilterValues()
        {
            Take = ListConstants.DefaultTakePerPage;
        }
        public string Search { get; set; }
        public string CategoryId { get; set; }
        public string ExportId { get; set; }
        public string SellerId { get; set; }
        public ProductSortOption? Sorting{ get; set; }
        public bool? IsAvailable { get; set; }
        public bool HasImage { get; set; }
        public bool IsActive { get; set; }
        public int Page { get; set; }
        public bool? HasParameters { get; set; }
        public bool? HasVendor { get; set; }
        public bool? HasOriginCountry { get; set; }
        public ModerationStatus? ModerationStatus { get; set; }
        public string ModeratorId { get; set; }
        public int Take { get; set; }
        public bool HasValues
        {
            get { return (Search != null || CategoryId != null || SellerId != null || ExportId != null); }
        }
    }
    public class ProductFilters
    {
        public List<HierarchySelectItem> Categories { get; set; }
        public List<SelectListItem> Exports { get; set; }
        public List<SelectListItem> Currencies { get; set; }
        public IEnumerable<SelectListItem> Sellers { get; set; }
        public IEnumerable<SelectListItem> Sorting { get; set; }
        public IEnumerable<SelectListItem> HasParameters { get; set; }
        public IEnumerable<SelectListItem> HasVendor { get; set; }
        public IEnumerable<SelectListItem> HasOriginCountry { get; set; }
        public IEnumerable<SelectListItem> IsAvailable { get; set; }
        public IEnumerable<SelectListItem> ModerationStatuses { get; set; }
        public IEnumerable<SelectListItem> Moderators { get; set; }
        public bool IsActive{ get; set; }
        public bool HasImage { get; set; }
        public string Search { get; set; }
        public int Take { get; set; }
        public int PagesCount { get; set; }
    }
}