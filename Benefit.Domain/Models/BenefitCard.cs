using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class BenefitCard
    {
        //written number
        [Required]
        [MaxLength(10)]
        [Index(IsUnique = true)]
        public string Id { get; set; }
        [Required]
        [MaxLength(16)]
        [Index(IsUnique = true)]
        public string NfcCode { get; set; }
        public bool IsTrinket { get; set; }
        [MaxLength(64)]
        public string HolderName { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
