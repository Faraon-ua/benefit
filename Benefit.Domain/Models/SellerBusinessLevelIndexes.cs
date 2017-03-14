using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Benefit.Domain.Models.Enums;

namespace Benefit.Domain.Models
{
    public class SellerBusinessLevelIndex
    {
        [Key, Column(Order = 0)]
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        [Key, Column(Order = 1)]
        [Required]
        public BusinessLevel? BusinessLevel { get; set; }
        public int Index { get; set; }
    }
}
