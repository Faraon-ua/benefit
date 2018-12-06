using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Address
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string FullName { get; set; }
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; }
        [MaxLength(128)]
        public string Email { get; set; }
        [MaxLength(256)]
        public string AddressLine { get; set; }
        public int? ZIP { get; set; }
        public bool IsDefault { get; set; }
        public int RegionId { get; set; }
        public virtual Region Region { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public virtual Seller Seller { get; set; }
    }
}
