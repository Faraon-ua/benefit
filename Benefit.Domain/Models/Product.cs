using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Data.Entity;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;

namespace Benefit.Domain.Models
{
    public class Product
    {
        public Product()
        {
            Images = new Collection<Image>();
            ProductOptions = new Collection<ProductOption>();
            ProductParameterProducts = new Collection<ProductParameterProduct>();
            Reviews = new Collection<Review>();
        }

        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        [Index]
        public string Name { get; set; }
        [Required]
        [MaxLength(128)]
        [Index(IsUnique = true)]
        public string UrlName { get; set; }
        [Index(IsUnique = true)]
        public int SKU { get; set; }
        public int? AvarageRating { get; set; } 
        [Required(AllowEmptyStrings = true, ErrorMessage = "Опис обовязковий для заповнення")]
        public string Description { get; set; }
        public double Price { get; set; }
        public bool IsWeightProduct { get; set; }
        public int? AvailableAmount { get; set; }
        #region advertisement
        public bool IsFeatured { get; set; }
        public bool IsNewProduct { get; set; }
        public int Order { get; set; }
        #endregion
        public bool IsActive { get; set; }
        public bool DoesCountForShipping { get; set; }
        public DateTime LastModified { get; set; }
        [MaxLength(64)]
        public string LastModifiedBy { get; set; }
        [MaxLength(160)]
        [Index]
        public string SearchTags { get; set; }
        [MaxLength(160)]
        public string ShortDescription { get; set; }
        [Required]
        [MaxLength(128)]
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }
        [Required]
        [MaxLength(128)]
        public string SellerId { get; set; }
        public virtual Seller Seller { get; set; }
        [MaxLength(128)]
        public string CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<ProductParameterProduct> ProductParameterProducts { get; set; }
        public virtual ICollection<ProductOption> ProductOptions { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        [NotMapped]
        public virtual ICollection<Review> ApprovedReviews {
            get
            {
                return Reviews.Where(entry => entry.IsActive && entry.Rating != null).ToList();
            } 
        }

        private KeyValuePair<bool, string>? _availableForPurchase = null;
        public KeyValuePair<bool, string> AvailableForPurchase(int regionId)
        {
            if (_availableForPurchase.HasValue)
            {
                return _availableForPurchase.Value;
            }
            string shippingRegions = null;
            using (var db = new ApplicationDbContext())
            {
                var isAvailable =
                    db.ShippingMethods.Any(
                        entry => entry.SellerId == SellerId && (entry.RegionId == RegionConstants.AllUkraineRegionId ||
                                                                 entry.RegionId == regionId));
                if (!isAvailable)
                {
                    shippingRegions = string.Join(",", db.ShippingMethods.Include(entry=>entry.Region).Where(entry=>entry.SellerId == SellerId).Select(entry => entry.Region.Name_ua).Distinct());
                }
                _availableForPurchase = new KeyValuePair<bool, string>(isAvailable, shippingRegions);
            }
            
            return _availableForPurchase.Value;
        }
    }
}
