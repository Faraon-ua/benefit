﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum PaymentType
    {
        [Description("По домовленності")]
        Agreement,
        [Description("Готівкою при отриманні")]
        Cash,
        [Description("Карткою Visa/MasterCard")]
        Acquiring,
        [Description("Бонусами")]
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
