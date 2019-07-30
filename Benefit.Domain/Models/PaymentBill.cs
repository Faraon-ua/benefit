using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class PaymentBill
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        public string Number { get; set; }
        public DateTime Time { get; set; }
        public int Type { get; set; }
        public double Sum { get; set; }
        public int Status { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
