using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Benefit.Domain.Models
{
    public enum OrderStatus
    {
        [Display(Name = "В очікуванні")]
        [Description("В очікуванні")]
        Created,
        [Display(Name = "В обробці")]
        [Description("В обробці")]
        Processed,
        [Display(Name = "Завершений")]
        [Description("Завершений")]
        Finished,
        [Display(Name = "Скасований")]
        [Description("Скасований")]
        Abandoned
    }

    public enum OrderType
    {
        BenefitSite,
        BenefitCard 
    }

    public class Order
    {
        public Order()
        {
            Id = Guid.NewGuid().ToString();
            Status = OrderStatus.Created;
            Transactions = new Collection<Transaction>();
            OrderProducts = new Collection<OrderProduct>();
            OrderProductOptions = new Collection<OrderProductOption>();
        }
        public string Id { get; set; }
        public int OrderNumber { get; set; }
        public double Sum { get; set; }
        public string Description { get; set; }
        public double PersonalBonusesSum { get; set; }
        public double PointsSum { get; set; }
        public string CardNumber { get; set; }
        [MaxLength(64)]
        public string ShippingName { get; set; }
        [MaxLength(256)]
        public string ShippingAddress { get; set; }
        public double ShippingCost { get; set; }
        public DateTime Time { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentType PaymentType { get; set; }
        public OrderStatus Status { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        [MaxLength(128)]
        public string SellerName { get; set; }
        public string SellerDiscountName { get; set; }
        public double? SellerDiscount { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        [MaxLength(64)]
        public string PersonnelName { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; } 
        public virtual ICollection<OrderProductOption> OrderProductOptions { get; set; }
        public virtual ICollection<OrderStatusStamp> OrderStatusStamps { get; set; }

        [NotMapped]
        public Transaction BonusPaymentTransaction
        {
            get { return Transactions.FirstOrDefault(entry => entry.Type == TransactionType.BonusesOrderPayment); }
        }
        [NotMapped]
        public bool IsRepeating { get; set; }
        [NotMapped]
        public string SellerPhone { get; set; }
        [NotMapped]
        public double SumWithDiscount{
            get { return Sum - SellerDiscount.GetValueOrDefault(0); }
        }
        public double GetOrderSum()
        {
            var sum = OrderProducts.Sum(
                entry =>
                    entry.ProductPrice*entry.Amount +
                    (entry.OrderProductOptions.Any()
                        ? entry.OrderProductOptions.Sum(option => option.ProductOptionPriceGrowth*option.Amount)
                        : entry.DbOrderProductOptions.Sum(option => option.ProductOptionPriceGrowth*option.Amount)));
            return sum;
        }
    }
}
