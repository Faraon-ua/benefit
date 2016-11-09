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
            Addresses = new Collection<Address>();
            Images = new Collection<Image>();
            Schedules = new Collection<Schedule>();
            ShippingMethods = new Collection<ShippingMethod>();
            SellerCategories = new Collection<SellerCategory>();
            ProductOptions = new Collection<ProductOption>();
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
        [Index(IsUnique = true)]
        public string UrlName { get; set; }
        [Required]
        [MaxLength(16)]
        public string CatalogButtonName { get; set; }
        public bool IsActive { get; set; }
        public bool IsBenefitCardActive { get; set; }
        public bool IsBonusesPaymentActive { get; set; }
        public bool IsAcquiringActive { get; set; }
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
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<ShippingMethod> ShippingMethods { get; set; }
        public ICollection<ProductOption> ProductOptions { get; set; }
    }
}
