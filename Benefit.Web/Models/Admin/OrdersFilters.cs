using System.Collections.Generic;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;

namespace Benefit.Web.Models.Admin
{
    public class AdminOrdersFilters : OrdersFilters
    {
        public AdminOrdersFilters()
        {
            PaymentType = string.Empty;
        }
        public IEnumerable<SelectListItem> Sellers { get; set; }
        public IEnumerable<SelectListItem> Sorting { get; set; }
    }
}