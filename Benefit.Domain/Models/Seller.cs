using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class Seller
    {
        public Seller()
        {
            Currencies = new Collection<Currency>();
        }

        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(128)]
        [Index]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [MaxLength(128)]
        public string UrlName { get; set; }
        [Required]
        [MaxLength(16)]
        public string CatalogButtonName { get; set; }
        public bool IsActive { get; set; }
        public bool IsBenefitCardActive { get; set; }
        public bool HasEcommerce { get; set; }
        public int TotalDiscount { get; set; }
        public double UserDiscount { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        [MaxLength(128)]
        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }
        [MaxLength(128)]
        public string BenefitCardReferalId { get; set; }
        public virtual ApplicationUser BenefitCardReferal { get; set; }
        [MaxLength(128)]
        public string WebSiteReferalId { get; set; }
        public virtual ApplicationUser WebSiteReferal { get; set; }
        public virtual ICollection<SellerCategory> SellerCategories { get; set; }
        public virtual ICollection<Currency> Currencies { get; set; }

        //        public ICollection<Address> Addresses { get; set; }
        //        public ICollection<Schedule> Schedules { get; set; }
    }
}
