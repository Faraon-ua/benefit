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
        [Description("В очікуванні")]
        Created,
        [Description("В обробці")]
        Processed,
        [Description("Завершений")]
        Finished,
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
        [MaxLength(32)]
        public string ShippingName { get; set; }
        [MaxLength(256)]
        public string ShippingAddress { get; set; }
        public double ShippingCost { get; set; }
        public DateTime Time { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentType PaymentType { get; set; }
        public OrderStatus Status { get; set; }
        [MaxLength(64)]
        public string StatusComment { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        [MaxLength(128)]
        public string SellerName { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        [MaxLength(64)]
        public string PersonnelName { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; } 
        public virtual ICollection<OrderProductOption> OrderProductOptions { get; set; }

        public double GetOrderSum()
        {
            var sum = OrderProducts.Sum(
                    entry =>
                        entry.ProductPrice * entry.Amount +
                        entry.OrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount));
            return sum;
        }
    }
}
