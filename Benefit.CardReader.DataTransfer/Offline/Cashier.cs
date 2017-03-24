using System;

namespace Benefit.CardReader.DataTransfer.Offline
{
    [Serializable]
    public class Cashier
    {
        public string CardNfc { get; set; }
        public string Name { get; set; }
        public string SellerName { get; set; }
    }
}
