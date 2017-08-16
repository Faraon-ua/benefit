using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum TransactionType
    {
        [Description("Персональні бонуси за замовлення")]
        PersonalSiteBonus,
        [Description("Персональні бонуси за замовлення Benefit Card")]
        PersonalBenefitCardBonus,
        [Description("Бонуси за запрошення")]
        MentorBonus,
        [Description("Переказ бонусів на інший рахунок")]
        Transfer,
        [Description("Бонуси за VIP")]
        VIPBonus,
        VIPSellerBonus,
        DirectorBonus,
        SilverBonus,
        GoldBonus,
        SellerInvolvementBonus,
        [Description("Повернення бонусів за редагування замовлення")]
        OrderRefund,
        [Description("Оплата за замовлення")]
        BonusesOrderPayment,
        [Description("Персональні бонуси за скасування замовлення")]
        BonusesOrderAbandonedPayment,
        PersonalMonthAggregate,
        Custom,
        [Description("Додаткове нарахування за бізнес рівень")]
        BusinessLevel,
        [Description("Бонуси по акції")]
        Promotion,
        [Description("Розрахунок бонусами через Benefit Card")]
        BenefitCardBonusesPayment
    }
    public class Transaction
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public TransactionType Type { get; set; }
        public double Bonuses { get; set; }
        public double Commission { get; set; }
        public double BonusesBalans { get; set; }
        public DateTime Time { get; set; }
        public string Description { get; set; }

        [MaxLength(128)]
        public string OrderId { get; set; }
        public Order Order { get; set; }

        [MaxLength(128)]
        public string PayerId { get; set; }
        public ApplicationUser Payer { get; set; }

        [MaxLength(128)]
        public string PayeeId { get; set; }
        public ApplicationUser Payee { get; set; }
    }
}
