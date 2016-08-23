using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class Address
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(32)]
        public string Country { get; set; }
        [Required]
        [MaxLength(64)]
        public string Region { get; set; }
        [Required]
        [MaxLength(64)]
        [Index]
        public string City { get; set; }
        [Required]
        [MaxLength(256)]
        public string AddressLine { get; set; }
        public int ZIP { get; set; }
        public bool IsDefault { get; set; }
        public ApplicationUser User { get; set; }
        public Seller Seller { get; set; }
    }
}
