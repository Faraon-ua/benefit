﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Benefit.Common.Constants;

namespace Benefit.Domain.Models
{
    public class ProductComparer : IEqualityComparer<Product>
    {
        public bool Equals(Product x, Product y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Product obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public enum ProductAvailabilityState
    {
        [Description("#73d36a")]
        [Display(Name = "В наявності", Description = "fa-check")]
        Available,
        [Display(Name = "Завжди в наявності", Description = "fa-check")]
        [Description("#73d36a")]
        AlwaysAvailable,
        [Display(Name = "Немає в наявності", Description = "fa-times")]
        [Description("#333333")]
        NotInStock,
        [Display(Name = "Під замовлення", Description = "fa-check")]
        [Description("#fe6600")]
        OnDemand,
        [Display(Name = "Закінчується", Description = "fa-exclamation-circle")]
        [Description("#fe6600")]
        Ending
    }

    public enum ModerationStatus
    {
        [Display(Name = "Промодерований")]
        Moderated,
        [Display(Name = "На модерації")]
        IsModerating,
        [Display(Name = "Невалідний контент")]
        UnappropriateContent,
        [Display(Name = "Корзина")]
        Trash
    }

    public enum ComputedProductAvailabilityState
    {
        Available,
        AvailableInOtherRegion,
        NotAvailable
    }

    public class ProductAvailability
    {
        public ProductAvailability()
        {
            Regions = new Dictionary<string, string>();
        }
        public ComputedProductAvailabilityState State { get; set; }
        public Dictionary<string, string> Regions { get; set; }
    }

    public class Product
    {
        public Product()
        {
            Images = new Collection<Image>();
            ProductOptions = new Collection<ProductOption>();
            ProductParameterProducts = new Collection<ProductParameterProduct>();
            Reviews = new Collection<Review>();
            Promotions = new Collection<Promotion>();
            Favorites = new Collection<Favorite>();
            ExportProducts = new Collection<ExportProduct>();
        }

        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string ExternalId { get; set; }
        [Required]
        [MaxLength(256)]
        [Index]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(128)]
        [Index]
        public string UrlName { get; set; }
        [Index(IsUnique = true)]
        public int SKU { get; set; }
        [MaxLength(128)]
        public string Vendor { get; set; }
        [MaxLength(64)]
        public string OriginCountry { get; set; }
        public int? AvarageRating { get; set; }
        [Required(AllowEmptyStrings = true, ErrorMessage = "Опис обовязковий для заповнення")]
        public string Description { get; set; }
        public double Price { get; set; }
        public double? OldPrice { get; set; }
        public double? WholesalePrice { get; set; }
        public int? WholesaleFrom { get; set; }
        public bool IsWeightProduct { get; set; }
        public ProductAvailabilityState AvailabilityState { get; set; }
        public int? AvailableAmount { get; set; }
        #region advertisement
        public bool IsFeatured { get; set; }
        public bool IsNewProduct { get; set; }
        public DateTime AddedOn { get; set; }
        public int Order { get; set; }
        #endregion
        public bool IsActive { get; set; }
        public bool IsImported { get; set; }
        public bool DoesCountForShipping { get; set; }
        public DateTime LastModified { get; set; }
        [MaxLength(64)]
        public string LastModifiedBy { get; set; }
        #region Moderation
        [MaxLength(250)]
        public string Comment { get; set; }
        public ModerationStatus ModerationStatus { get; set; }
        #endregion 
        [MaxLength(160)]
        [Index]
        //internal site search
        public string SearchTags { get; set; }
        [MaxLength(100)]
        public string AltText { get; set; }
        [MaxLength(210)]
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
        public virtual ICollection<Promotion> Promotions { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<ExportProduct> ExportProducts { get; set; }

        [NotMapped]
        public int SearchRank { get; set; }

        [NotMapped]
        public virtual ICollection<Review> ApprovedReviews
        {
            get
            {
                return Reviews.Where(entry => entry.IsActive && entry.Rating != null).ToList();
            }
        }

        [NotMapped]
        public ICollection<Localization> Localizations { get; set; }

        private ProductAvailability _availableForPurchase = null;
        public ProductAvailability AvailableForPurchase(int regionId)
        {
            if (_availableForPurchase != null)
            {
                return _availableForPurchase;
            }
            var result = new ProductAvailability();
            if (AvailabilityState == ProductAvailabilityState.NotInStock ||
                (AvailabilityState == ProductAvailabilityState.Available && AvailableAmount == 0))
            {
                result.State = ComputedProductAvailabilityState.NotAvailable;
            }
            else
            {
                var isAvailable =
                    Seller.ShippingMethods.Any(
                        entry => entry.RegionId == RegionConstants.AllUkraineRegionId || entry.RegionId == regionId);
                if (!isAvailable)
                {
                    result.Regions =
                        Seller.ShippingMethods.Select(entry => entry.Region).Distinct(new RegionComparer()).ToDictionary(entry => entry.Id.ToString(),
                            entry => entry.Name_ua);
                    result.State = ComputedProductAvailabilityState.AvailableInOtherRegion;
                }
                else
                {
                    result.State = ComputedProductAvailabilityState.Available;
                }
            }
            return result;
        }
    }
}
