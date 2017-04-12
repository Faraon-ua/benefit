using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Benefit.Common.Constants;

namespace Benefit.Domain.Models
{
    public class Product
    {
        public Product()
        {
            Images = new Collection<Image>();
            ProductOptions = new Collection<ProductOption>();
            ProductParameterProducts = new Collection<ProductParameterProduct>();
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

        public KeyValuePair<bool, string> AvailableForPurchase(int regionId)
        {
            string shippingRegions = null;
            var isAvailable =
                Seller.ShippingMethods.Any(
                    entry => entry.RegionId == RegionConstants.AllUkraineRegionId ||
                        entry.RegionId == regionId);
            if (!isAvailable)
            {
                shippingRegions = string.Join(",", Seller.ShippingMethods.Select(entry => entry.Region.Name_ua).Distinct());
            }

            return new KeyValuePair<bool, string>(isAvailable, shippingRegions);
        }
    }
}
