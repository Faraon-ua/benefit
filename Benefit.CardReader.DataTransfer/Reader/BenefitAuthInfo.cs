namespace Benefit.CardReader.DataTransfer.Reader
{
    public class BenefitAuthInfo
    {
        public string CashierNfc { get; set; }
        public string CashierName { get; set; }
        public string UserNfc { get; set; }
        public string UserName { get; set; }
        public string UserCard { get; set; }
        public string SellerName { get; set; }
        public bool ShowBill { get; set; }
        public bool ShowChargeBonuses { get; set; }
        public bool ShowKeyboard { get; set; }
    }
}
