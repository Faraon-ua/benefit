using System.Collections.Generic;

namespace Benefit.DataTransfer.ApiDto.Rozetka
{
    public class UpdateOrderIngest
    {
        public int status { get; set; }
        public string seller_comment { get; set; }
        public string ttn { get; set; }
    }

    public class OrderProductQuantityIngest
    {
        public string id { get; set; }
        public int quantity { get; set; }
    }
    public class UpdateOrderPurchasesIngest
    {
        public UpdateOrderPurchasesIngest()
        {
            purchases = new List<OrderProductQuantityIngest>();
        }
        public List<OrderProductQuantityIngest> purchases { get; set; }
    }
}
