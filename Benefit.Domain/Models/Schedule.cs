using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Schedule
    {
        public string Id { get; set; }
        [Required]
        public string Day { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
