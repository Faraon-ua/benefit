using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Transaction
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(64)]
        public string Description { get; set; }
        public double? Points { get; set; }
        public double? Bonuses { get; set; }
        public double? BonusesBalans { get; set; }
        public DateTime Time { get; set; }
//        public Status? Qualification { get; set; }
        [MaxLength(128)]
        public string OrderId { get; set; }
        public Order Order { get; set; }
    }
}
