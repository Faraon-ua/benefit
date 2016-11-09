using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum OrderStatus
    {
        Created,
        Processed,
        Finished
    }

    public enum OrderType
    {
        BenefitSite,
        BenefitCard,
        Bonus
    }
    public class Order
    {
        public string Id { get; set; }
        public double Sum { get; set; }
        public double PersonalBonusesSum { get; set; }
        public double PointsSum { get; set; }
        public string CardNumber { get; set; }
        public DateTime Time { get; set; }
        public OrderType OrderType { get; set; }
        public OrderStatus Status { get; set; }
        public virtual Seller Seller { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
