using Benefit.Common.Constants;
using Benefit.Domain.Models.Base;
using Benefit.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;

namespace Benefit.Domain.Models
{
    public enum SellerEcommerceTemplate
    {
        Default,
        MegaShop,
        EcolifeFurniture
    }

    public class Seller : BaseDomainModel
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
            Personnels = new Collection<Personnel>();
            Promotions = new Collection<Promotion>();
            InfoPages = new Collection<InfoPage>();
            Reviews = new Collection<Review>();
        }
       
        [Required]
        [MaxLength(128)]
        [Index]
        public string Name { get; set; }
        [Required]
        [MaxLength(128)]
        public string LegalName { get; set; }
        [Required]
        [MaxLength(500)]
        public string LegalDescription { get; set; }
        public string Description { get; set; }
        [MaxLength(50)]
        public string PrimaryRegionName { get; set; }
        public int PrimaryRegionId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Widget { get; set; }  
        public string CustomScripts { get; set; }  
        public double CurrentBill { get; set; }
        public double GreyZone { get; set; }
        public DateTime? BlockOn { get; set; }
        //to filter in sellers catalog
        [MaxLength(50)]
        public string CategoryName { get; set; }
        [MaxLength(160)]
        //internal site search
        public string SearchTags { get; set; }
        [MaxLength(100)]
        public string AltText { get; set; }
        public string ShippingDescription { get; set; }
        [Required]
        [MaxLength(128)]
        [Index(IsUnique = true)]
        public string UrlName { get; set; }
        [MaxLength(128)]
        public string RedirectUrl { get; set; }
        [MaxLength(60)]
        public string Domain { get; set; }
        public int? AvarageRating { get; set; }
        public SellerStatus Status { get; set; }
        public bool SafePurchase { get; set; }
        #region colors
        [MaxLength(16)]
        public string HeaderColor { get; set; }
        [MaxLength(16)]
        public string MenuColor { get; set; }
        [MaxLength(16)]
        public string BodyColor { get; set; }
        [MaxLength(16)]
        public string AdverticementColor { get; set; }
        [MaxLength(16)]
        public string FooterColor { get; set; }
        [MaxLength(16)]
        public string CopyrightColor { get; set; }
        [MaxLength(16)]
        public string CabinetColor { get; set; }
        [MaxLength(16)]
        public string CatalogColor { get; set; }
        [MaxLength(16)]
        public string FavoritesColor { get; set; }
        [MaxLength(16)]
        public string CardColor { get; set; }
        #endregion
        #region SEO
        [MaxLength(50)]
        public string GoogleSiteVerificationToken { get; set; }
        [MaxLength(80)]
        public string Title { get; set; }
        [MaxLength(250)]
        public string SeoSuffix { get; set; }
        [MaxLength(160)]
        public string ShortDescription { get; set; }
        #endregion
        #region Terminal
        public bool TerminalOrderNotification { get; set; }
        public bool TerminalBillEnabled { get; set; }
        public bool TerminalKeyboardEnabled { get; set; }
        public bool TerminalBonusesPaymentActive { get; set; }
        [MaxLength(32)]
        public string TerminalLogin { get; set; }
        [MaxLength(32)]
        public string TerminalPassword { get; set; }
        [MaxLength(32)]
        public string TerminalLicense { get; set; }
        public DateTime? TerminalLastOnline { get; set; }
        public bool IsObsoleteTerminal { get; set; }
        #endregion
        #region Statistics
        public bool LogRequests { get; set; }
        #endregion
        //hours
        public int? RepeatingTransactionInterval { get; set; }
        [Required]
        [MaxLength(16)]
        public string CatalogButtonName { get; set; }
        [MaxLength(64)]
        public string OnlineOrdersPhone { get; set; }
        public string Contacts { get; set; }
        public bool IsActive { get; set; }
        public bool IsBenefitCardActive { get; set; }
        //todo: move payment types to separate table
        [DisplayName("Передоплата")]
        public bool IsPrePaidPaymentActive { get; set; }
        [DisplayName("Післяплата")]
        public bool IsPostPaidPaymentActive { get; set; }
        [DisplayName("Оплата Бонусами")]
        public bool IsBonusesPaymentActive { get; set; }
        [DisplayName("Оплата Готівкою")]
        public bool IsCashPaymentActive { get; set; }
        [DisplayName("Еквайринг")]
        public bool IsAcquiringActive { get; set; }
        public bool IsFeatured { get; set; }
        public bool AreProductsFeatured { get; set; }
        public bool GenerateRecommendedProducts { get; set; }
        public bool GenerateFeaturedProducts { get; set; }
        public bool HasEcommerce { get; set; }
        public SellerEcommerceTemplate? EcommerceTemplate { get; set; }
        public int TotalDiscount { get; set; }
        public double UserDiscount { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        [MaxLength(128)]
        public string OwnerId { get; set; }
        //для рассчета процента тому, кто подключил заведение
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }
        [MaxLength(128)]
        public string BenefitCardReferalId { get; set; }
        public virtual ApplicationUser BenefitCardReferal { get; set; }
        [MaxLength(128)]
        public string WebSiteReferalId { get; set; }
        public virtual ApplicationUser WebSiteReferal { get; set; }
        [MaxLength(128)]
        public string AssociatedSellerId { get; set; }
        public Seller AssociatedSeller { get; set; }
        public virtual ICollection<Seller> AssociatedSellers { get; set; }
        public virtual ICollection<SellerCategory> SellerCategories { get; set; }
        public virtual ICollection<Category> MappedCategories { get; set; }
        public virtual ICollection<Currency> Currencies { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Banner> Banners { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<ShippingMethod> ShippingMethods { get; set; }
        public virtual ICollection<SellerBusinessLevelIndex> BusinessLevelIndexes { get; set; }
        public ICollection<ProductOption> ProductOptions { get; set; }
        public ICollection<Personnel> Personnels { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Promotion> Promotions { get; set; }
        public virtual ICollection<NotificationChannel> NotificationChannels { get; set; }
        public virtual ICollection<InfoPage> InfoPages { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<PaymentBill> PaymentBills { get; set; }

        [NotMapped]
        public ICollection<Category> FeaturedCategories { get; set; } 
        [NotMapped]
        public ICollection<Product> FeaturedProducts { get; set; }
        [NotMapped]
        public ICollection<Product> PromotionProducts { get; set; }
        [NotMapped]
        public ICollection<Product> NewProducts { get; set; }
        [NotMapped]
        public ICollection<Product> RecommendedProducts { get; set; }

        [NotMapped]
        public virtual string LogoUrl
        {
            get
            {
                if (Images.Any())
                {
                    var logo = Images.FirstOrDefault(entry => entry.ImageType == ImageType.SellerLogo);
                    return logo == null ? string.Empty : logo.ImageUrl;
                }
                return null;
            }
        }

        [NotMapped]
        public virtual ICollection<Review> ApprovedReviews
        {
            get
            {
                return Reviews.Where(entry => entry.IsActive && entry.Rating != null).ToList();
            }
        }
        [NotMapped]
        public static bool IsSellerAdmin
        {
            get
            {
                return CurrentAuthorizedSellerId != null;
            }
        }
        [NotMapped]
        public static string CurrentAuthorizedSellerId
        {
            get
            {
                return HttpContext.Current.Session[DomainConstants.SellerSessionIdKey] == null
                    ? null
                    : HttpContext.Current.Session[DomainConstants.SellerSessionIdKey].ToString();
            }
        }
        [NotMapped]
        public static string CurrentAuthorizedSellerName
        {
            get
            {
                return HttpContext.Current.Session[DomainConstants.SellerSessionNameKey] == null
                    ? null
                    : HttpContext.Current.Session[DomainConstants.SellerSessionNameKey].ToString();
            }
        }
    }
}
