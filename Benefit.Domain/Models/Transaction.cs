using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum TransactionType
    {
        [Description("Кешбек бонуси за замовлення")]
        CashbackBonus,
        [Description("Кешбек бонуси за партнерською програмою")]
        CashbackMentorBonus,
        [Description("Повернення бонусів за редагування замовлення")]
        OrderRefund,
        [Description("Оплата бонусами за замовлення")]
        BonusesOrderPayment,
        [Description("Бонуси за скасування замовлення")]
        BonusesOrderAbandonedPayment,
        [Description("Поповнення рахунку")]
        SellerBillRefill,
        [Description("Переказ бонусів на інший рахунок")]
        Transfer,
        [Description("Зарахування бонуси доступні за замовлення")]
        HangingToGeneral
    }
    public class Transaction
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public TransactionType Type { get; set; }
        //for moving bonuses from hanging to general
        public bool IsProcessed { get; set; }
        public double Bonuses { get; set; }
        public double BonusesBalans { get; set; }
        public DateTime Time { get; set; }
        public string Description { get; set; }
        [MaxLength(128)]
        public string OrderId { get; set; }
        public Order Order { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        [MaxLength(128)]
        public string PayerId { get; set; }
        public ApplicationUser Payer { get; set; }
        [MaxLength(128)]
        public string PayeeId { get; set; }
        public ApplicationUser Payee { get; set; }
    }
}
