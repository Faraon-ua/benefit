using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class BenefitCard
    {
        [MaxLength(10)]
        [Key, Column(Order = 0)]
        public string Id { get; set; }
        [MaxLength(16)]
        [Key, Column(Order = 1)]
        public string NfcCode { get; set; }
        public bool IsTrinket { get; set; }
        [MaxLength(64)]
        public string HolderName { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [MaxLength(128)]
        public string ReferalUserId { get; set; }
        public ApplicationUser ReferalUser { get; set; }
    }
}
