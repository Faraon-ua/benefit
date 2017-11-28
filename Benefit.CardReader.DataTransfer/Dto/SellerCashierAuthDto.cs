namespace Benefit.CardReader.DataTransfer.Dto
{
    public class SellerCashierAuthDto
    {
        public string SellerName {get; set; }
        public string CashierName {get; set; }
        public bool ShowBill { get; set; }
        public bool ShowBonusesPayment { get; set; }
        public bool ShowKeyboard { get; set; }
    }
}
