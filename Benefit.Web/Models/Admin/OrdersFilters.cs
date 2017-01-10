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
        public OrderType NavigationType { get; set; }
        public PaginatedList<Order> Orders { get; set; } 
    }
}