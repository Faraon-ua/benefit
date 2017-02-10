using System;

namespace Benefit.CardReader.DataTransfer.Dto
{
    public class CardOrderDto
    {
        public DateTime Time { get; set; }
        public string SellerName { get; set; }
        public double Sum { get; set; }
    }
}
