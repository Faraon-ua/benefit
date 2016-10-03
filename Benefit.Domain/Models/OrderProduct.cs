﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class OrderProduct
    {
        [Key, Column(Order = 0)]
        [MaxLength(128)]
        public string OrderId { get; set; }
        public virtual Order Order { get; set; }
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int? Amount { get; set; }
    }
}
