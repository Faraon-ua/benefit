using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;

namespace Benefit.Web.Models.Admin
{
    public class OrdersFilters
    {
        public OrdersFilters()
        {
            PaymentType = string.Empty;
        }
        public int? OrderNumber { get; set; }
        public string ProductName { get; set; }
        public int? Status { get; set; }
        public string DateRange { get; set; }
        public string ClientName { get; set; }
        public string Phone { get; set; }
        public string Comment { get; set; }
        public double TotalSum{ get; set; }
        public double? SumFrom { get; set; }
        public double? SumTo { get; set; }

        public string PaymentType { get; set; }
        public int Number { get; set; }
        public string PersonnelName { get; set; }
        public bool ClientGrouping { get; set; }
        public OrderSortOption Sort { get; set; } 
        public string SellerId { get; set; }
        //public OrderType NavigationType { get; set; }
        public PaginatedList<Order> Orders { get; set; } 
    }
}