﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace Benefit.Domain.Models
{
    public class Promotion
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime? Start { get; set; }
        [Required]
        public DateTime? End { get; set; }
        public short? StartTime { get; set; }
        public short? EndTime { get; set; }
        public string ProductId { get; set; }
        public Product Product { get; set; }
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        [Required]
        public double? DiscountFrom { get; set; }
        [Required]
        public double? DiscountValue { get; set; }
        public bool IsValuePercent { get; set; }
        public bool IsActive { get; set; }
        public bool IsBonusDiscount { get; set; }
        public bool IsCurrentAccountBonusPromotion { get; set; }
        public bool IsMentorPromotion { get; set; }
        public bool ShouldBeVisibleInStructure { get; set; }
        public int Level { get; set; }

        public ICollection<PromotionAccomplishment> PromotionAccomplishments { get; set; } 

        [NotMapped]
        public DateTime StartTimeDt { get; set; }
        [NotMapped]
        public DateTime EndTimeDt { get; set; }
    }
}
