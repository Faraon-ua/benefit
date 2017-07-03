using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Benefit.Common.Constants;
using Benefit.Domain.Models.Enums;

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
            Personnels = new Collection<Personnel>();
            Promotions = new Collection<Promotion>();
        }

        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(128)]
        [Index]
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [MaxLength(160)]
        public string ShortSescription { get; set; }
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
        public int? AvarageRating { get; set; }
        public SellerStatus Status { get; set; }
        public bool TerminalOrderNotification { get; set; }
        public bool TerminalBillEnabled { get; set; }
        [MaxLength(32)]
        public string TerminalLogin { get; set; }
        [MaxLength(32)]
        public string TerminalPassword { get; set; }
        public DateTime TerminalLastOnline { get; set; }
        //hours
        public int? RepeatingTransactionInterval { get; set; }
        [Required]
        [MaxLength(16)]
        public string CatalogButtonName { get; set; }
        [MaxLength(64)]
        public string OnlineOrdersPhone { get; set; }
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
        public bool HasEcommerce { get; set; }
        public int TotalDiscount { get; set; }
        public double UserDiscount { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        [MaxLength(128)]
        public string OwnerId { get; set; }
        //для рассчета процента тому, кто подключил заведение
        public double PointsAccount { get; set; }
        public double HangingPointsAccount { get; set; }
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }
        [MaxLength(128)]
        public string BenefitCardReferalId { get; set; }
        public virtual ApplicationUser BenefitCardReferal { get; set; }
        [MaxLength(128)]
        public string WebSiteReferalId { get; set; }
        public virtual ApplicationUser WebSiteReferal { get; set; }
        public virtual ICollection<SellerCategory> SellerCategories { get; set; }
        public virtual ICollection<Category> MappedCategories { get; set; }
        public virtual ICollection<Currency> Currencies { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<ShippingMethod> ShippingMethods { get; set; }
        public virtual ICollection<SellerBusinessLevelIndex> BusinessLevelIndexes { get; set; }
        public ICollection<ProductOption> ProductOptions { get; set; }
        public ICollection<Personnel> Personnels { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Promotion> Promotions { get; set; }
        public virtual ICollection<NotificationChannel> NotificationChannels { get; set; }
        [NotMapped]
        public virtual ICollection<Review> ApprovedReviews
        {
            get
            {
                return Reviews.Where(entry => entry.IsActive && entry.Rating != null).ToList();
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
