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
}
