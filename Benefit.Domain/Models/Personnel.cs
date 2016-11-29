using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Personnel
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(32)]
        public string Name { get; set; }
        [MaxLength(16)]
        public string Phone { get; set; }
        [MaxLength(10)]
        public string CardNumber { get; set; }
        [MaxLength(10)]
        public string NFCCardNumber { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
