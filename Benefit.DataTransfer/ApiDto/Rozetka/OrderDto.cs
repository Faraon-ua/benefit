using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.DataTransfer.ApiDto.Rozetka
{
    public class OrdersModelDto {
        public string success { get; set; }
        public OrdersListDto content { get; set; }
    }
    public class MetaModelDto
    {
        public int pageCount { get; set; }
        public int currentPage { get; set; }
    }

    public class OrdersListDto
    {
        public List<OrderDto> orders { get; set; }
        public MetaModelDto _meta { get; set; }
    }
    public class DeliveryCityDto
    {
        public string title { get; set; }
    }
    public class DeliveryDto
    {
        public string delivery_service_name { get; set; }
        public string recipient_title { get; set; }
        public string place_street { get; set; }
        public string place_house { get; set; }
        public string place_flat { get; set; }
        public double? cost { get; set; }
        public DeliveryCityDto city { get; set; }
    }
    public class OrderProductDto
    {
        public string id { get; set; }
        public double price { get; set; }
        //public double cost { get; set; }
        public double quantity { get; set; }
        public string item_name { get; set; }
        public string sellerId { get; set; }
    }
    public class OrderDto
    {
        public string id { get; set; }
        public string created { get; set; }
        public double amount { get; set; }
        public double cost { get; set; }
        public string comment { get; set; }
        public string user_phone { get; set; }
        public string payment_type { get; set; }
        public int status { get; set; }
        public List<OrderProductDto> purchases { get; set; }
        public DeliveryDto delivery { get; set; }
    }
}
