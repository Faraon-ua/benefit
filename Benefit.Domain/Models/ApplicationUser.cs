using System;

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
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        [Required]
        public int ExternalNumber { get; set; }
        [Required]
        public int ReferalNumber { get; set; }
        public bool B2BDoubleReward { get; set; }
        public DateTime RegisteredOn { get; set; }
//        public Seller Seller { get; set; }
    }
}
