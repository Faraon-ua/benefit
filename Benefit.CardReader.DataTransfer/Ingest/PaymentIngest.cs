namespace Benefit.CardReader.DataTransfer.Ingest
{
    public class PaymentIngest
    {
        public double Sum { get; set; }
        public string BillNumber { get; set; }
        public string CashierNfc { get; set; }
        public string UserNfc { get; set; }
        public bool ChargedBonuses { get; set; }
    }
}
