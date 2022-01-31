using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.DataTransfer.ApiDto.Allo
{
    public class OrdersIngestArgs
    {
        public string orderId { get; set; }
        public string accepted_from { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
    }
    public class OrdersIngest
    {
        public string sessionId { get; set; }
        public OrdersIngestArgs args { get; set; }
    }

    public class UpdateOrderIngest
    {
        public string sessionId { get; set; }
        public UpdateOrderIngestArgs args { get; set; }

    }
    public class UpdateOrderIngestStatus
    {
        public int main_status { get; set; }
        public string cancel_status_id { get; set; }
    }
    public class UpdateOrderIngestOrder
    {
        public string id { get; set; }
        public string tracking_number { get; set; }
        public UpdateOrderIngestStatus status { get; set; }
        public string note { get; set; }
        public string updated_date { get; set; }
    }
    public class UpdateOrderIngestArgs
    {
        public List<UpdateOrderIngestOrder> orders { get; set; }
        public int total_records { get { return 1; } }
    }
}
