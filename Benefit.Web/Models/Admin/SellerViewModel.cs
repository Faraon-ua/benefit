﻿using Benefit.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Web.Models.Admin
{
    public class SellerViewModel
    {
        public Seller Seller { get; set; }
        [Required]
        public int OwnerExternalId { get; set; }
        public int? WebSiteReferaExternalId { get; set; }
        public int? BenefitCardReferaExternalId { get; set; }
    }
}