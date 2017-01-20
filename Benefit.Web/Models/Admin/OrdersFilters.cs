﻿using System.Collections.Generic;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models;

namespace Benefit.Web.Models.Admin
{
    public class OrdersFilters
    {
        public OrdersFilters()
        {
            NavigationType = OrderType.BenefitSite;
        }
        public string ClientName { get; set; }
        public string DateRange { get; set; }
        public string SellerId { get; set; }
        public OrderType NavigationType { get; set; }
        public IEnumerable<SelectListItem> Sellers { get; set; }
        public PaginatedList<Order> Orders { get; set; } 
    }
}