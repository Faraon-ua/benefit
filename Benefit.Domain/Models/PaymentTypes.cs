using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum PaymentType
    {
        Cash,
        Emoney,
        Bonuses
    }
    public class PaymentTypes
    {
        public string Id { get; set; }
        public bool CashPayment { get; set; }
        public bool EmoneyPayment { get; set; }
        public bool BonusPayment { get; set; }
        [MaxLength(128)]
        public string LiqPay { get; set; }
    }
}
