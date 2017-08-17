using System;
using Newtonsoft.Json;

namespace Benefit.CardReader.DataTransfer.Ingest
{
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class PaymentIngest
    {
        public double Sum { get; set; }
        public string BillNumber { get; set; }
        public string CashierNfc { get; set; }
        public string UserNfc { get; set; }
        public bool ChargeBonuses { get; set; }
    }
}
