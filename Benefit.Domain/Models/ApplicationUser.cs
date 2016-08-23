using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Benefit.Domain.Models.Enums;

namespace Benefit.Domain.Models
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNet.Identity.EntityFramework;

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; }
        [Required]
        [MaxLength(64)]
        [Index]
        public string FullName { get; set; }
        [Index(IsUnique = true)]
        [MaxLength(10)]
        public string CardNumber { get; set; }
        [Required]
        [Index(IsUnique = true)]
        public int ExternalNumber { get; set; }
        public BusinessLevel? BusinessLevel { get; set; }
        public Status? Status { get; set; }
        public string ReferalId { get; set; }
        public ApplicationUser Referal { get; set; }
        public DateTime RegisteredOn { get; set; }
        public double CurrentBonusAccount { get; set; }
        public double CurrentPointsAccount { get; set; }
        public virtual ICollection<Seller> OwnedSellers { get; set; }
        public virtual ICollection<Seller> ReferedWebSiteSellers { get; set; }
        public virtual ICollection<Seller> ReferedBenefitCardSellers { get; set; }
    }
}
