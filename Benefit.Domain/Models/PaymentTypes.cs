using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class PaymentTypes
    {
        public string Id { get; set; }
        public bool CashPayment { get; set; }
        public bool EmoneyPayment { get; set; }
        public bool BonusPayment { get; set; }
        [MaxLength(64)]
        public string LiqPay { get; set; }
    }
}
