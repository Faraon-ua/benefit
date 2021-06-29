using System;
using System.ComponentModel.DataAnnotations;
using Benefit.Domain.Models;

namespace Benefit.Domain
{
    public class StatusStamp
    {
        public string Id { get; set; }
        [MaxLength(128)]
        public string OrderId { get; set; }
        public Order Order { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        public int Status { get; set; }
        public DateTime Time { get; set; }
        [MaxLength(32)]
        public string UpdatedBy { get; set; }
        [MaxLength(64)]
        public string Comment { get; set; }
    }
}
