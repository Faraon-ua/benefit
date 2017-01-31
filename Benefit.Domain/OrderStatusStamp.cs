using System;
using System.ComponentModel.DataAnnotations;
using Benefit.Domain.Models;

namespace Benefit.Domain
{
    public class OrderStatusStamp
    {
        public string Id { get; set; }
        [MaxLength(128)]
        [Required]
        public string OrderId { get; set; }
        public Order Order { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime Time { get; set; }
        [MaxLength(32)]
        public string UpdatedBy { get; set; }
        [MaxLength(64)]
        public string Comment { get; set; }
    }
}
