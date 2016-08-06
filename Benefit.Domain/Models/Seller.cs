using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Seller
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(128)]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsBenefitCardActive { get; set; }
        public bool HasEcommerce { get; set; }
        public double PointsPer100Uah { get; set; }
        public int TotalDiscount { get; set; }
        public int CustomerDiscount { get; set; }
      
//        public ICollection<Address> Addresses { get; set; }
//        public ICollection<Schedule> Schedules { get; set; }
        public ApplicationUser User { get; set; }
    }
}
