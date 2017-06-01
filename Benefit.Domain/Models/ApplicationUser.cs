using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Benefit.Domain.Models.Enums;

namespace Benefit.Domain.Models
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNet.Identity.EntityFramework;

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Orders = new Collection<Order>();
            BenefitCards = new Collection<BenefitCard>();
        }
        public bool IsActive { get; set; }
        public bool IsCardVerified { get; set; }
        [Required]
        [MaxLength(64)]
        [Index]
        public string FullName { get; set; }
        [Index]
        [MaxLength(10)]
        public string CardNumber { get; set; }
        [Index]
        [MaxLength(16)]
        public string NFCCardNumber { get; set; }
        //todo: add md5 with salt
        [MaxLength(8)]
        public string FinancialPassword { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; }
        [MaxLength(128)]
        public string Address { get; set; }
        [Remote("IsCardUnique", "Validation")]
        [Required]
        [Index(IsUnique = true)]
        public int ExternalNumber { get; set; }
        public BusinessLevel? BusinessLevel { get; set; }
        public Status? Status { get; set; }
        public string ReferalId { get; set; }
        public ApplicationUser Referal { get; set; }
        public DateTime RegisteredOn { get; set; }
        public double BonusAccount { get; set; }
        public double TotalBonusAccount { get; set; }
        public double CurrentBonusAccount { get; set; }
        public double HangingBonusAccount { get; set; }
        public double PointsAccount { get; set; }
        public double HangingPointsAccount { get; set; }
        [MaxLength(64)]
        public string Avatar { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Seller> OwnedSellers { get; set; }
        public virtual ICollection<Seller> ReferedWebSiteSellers { get; set; }
        public virtual ICollection<Seller> ReferedBenefitCardSellers { get; set; }
        public virtual ICollection<ApplicationUser> Partners { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<BenefitCard> BenefitCards { get; set; }
        public ICollection<PromotionAccomplishment> PromotionAccomplishments { get; set; }
    }

    public class ApplicationUserComparer : IEqualityComparer<ApplicationUser>
    {
        public bool Equals(ApplicationUser x, ApplicationUser y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(ApplicationUser obj)
        {
            return obj.GetHashCode();
        }
    }
}
