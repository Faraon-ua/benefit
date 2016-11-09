using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum TransactionType
    {
        PersonalSiteBonus,
        PersonalBenefitCardBonus,
        MentorBonus,
        Transfer,
        VIPBonus,
        DirectorBonus,
        SilverBonus,
        GoldBonus,
        SellerInvolvementBonus
    }
    public class Transaction
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public TransactionType Type { get; set; }
        public double? Bonuses { get; set; }
        public double? BonusesBalans { get; set; }
        public DateTime Time { get; set; }
//        public Status? Qualification { get; set; }
        [MaxLength(128)]
        public string PayerId { get; set; }
        public ApplicationUser Payer { get; set; }

        [MaxLength(128)]
        public string PayeeId { get; set; }
        public ApplicationUser Payee { get; set; }
    }
}
