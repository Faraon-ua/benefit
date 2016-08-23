using System;
using System.ComponentModel.DataAnnotations;
using Benefit.Domain.Models.Enums;

namespace Benefit.Domain.Models
{
    public class Transaction
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public double? Points { get; set; }
        public double? Bonuses { get; set; }
        public DateTime Time { get; set; }
        public Status? Qualification { get; set; }
        public string OrderId { get; set; }
    }
}
