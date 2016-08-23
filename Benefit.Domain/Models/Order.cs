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
        public DateTime Time { get; set; }
        public OrderType OrderType { get; set; }
        public OrderStatus Status { get; set; }
        public ApplicationUser User { get; set; }
        public Seller Seller { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
    }
}
