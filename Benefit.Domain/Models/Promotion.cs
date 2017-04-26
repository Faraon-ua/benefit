using System;

namespace Benefit.Domain.Models
{
    public class Promotion
    {
        public string Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ProductId { get; set; }
        public Product Product { get; set; }
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        public double InstantDiscountFrom { get; set; }
        public double InstantDiscountValue { get; set; }
    }
}
