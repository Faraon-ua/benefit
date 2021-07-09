using System.Collections.Generic;

namespace Benefit.DataTransfer.ApiDto.Allo
{
    public class UpdateOrderDto
    {
        public string id { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
    public class UpdateOrdersDto
    {
        public List<UpdateOrderDto> orders { get; set; }
    }
    public class OrderDto
    {
        public const int limit = 50;

        public string id { get; set; }
        public string note { get; set; }
        public string created_date { get; set; }
        public string payment_type { get; set; }
        public string payment_type_id { get; set; }
        public StatusDto status { get; set; }
        public ICollection<OrderProductDto> products { get; set; }
        public CustomerDto customer { get; set; }
        public ShippingDto shipping { get; set; }
    }
    public class StatusDto
    {
        public int status { get; set; }
    }
    public class OrderProductDto
    {
        public string name { get; set; }
        public string sku { get; set; }
        public double quantity { get; set; }
        public double price { get; set; }
        public double amount { get; set; }
    }

    public class CustomerDto
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string telephone { get; set; }
    }
    public class ShippingDto
    {
        public string type { get; set; }
        public string city { get; set; }
        public string region_name { get; set; }
        public string price { get; set; }
        public string tracking_number { get; set; }
        public StockDto stock { get; set; }
        public AddressDto address { get; set; }
    }
    public class StockDto
    {
        public string name { get; set; }
    }
    public class AddressDto
    {
        public string city { get; set; }
        public string street { get; set; }
        public string house { get; set; }
        public string apartment { get; set; }
    }
    public class OrdersDto
    {
        public ICollection<OrderDto> orders { get; set; }
        public int total_records { get; set; }
    }
}
